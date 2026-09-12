using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ai_dev_asst_api.Integrations.TicketProviders.Models;

namespace ai_dev_asst_api.Integrations.TicketProviders;

public class AzureDevOpsProvider : ITicketProvider
{
    private readonly HttpClient _http;
    private readonly string _organization;
    private readonly string _project;
    private readonly string _pat;

    public string ProviderName => "Azure DevOps";

    public AzureDevOpsProvider(IConfiguration config)
    {
        _organization = config["AzureDevOps:Organization"]!;
        _project = config["AzureDevOps:Project"]!;
        _pat = config["AzureDevOps:Pat"]!;

        _http = new HttpClient();
        var authBytes = Encoding.ASCII.GetBytes($":{_pat}");
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
    }

    public async Task<RawTicket> GetTicketAsync(string ticketId)
    {
        // Get work item with all fields and relations
        var url = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems/{ticketId}?$expand=all&api-version=7.1";
        var response = await _http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new ArgumentException($"Failed to fetch ticket {ticketId}: {response.StatusCode} - {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var fields = doc.RootElement.GetProperty("fields");

        var ticket = new RawTicket
        {
            Id = ticketId,
            Title = GetField(fields, "System.Title"),
            Description = GetField(fields, "System.Description"),
            AcceptanceCriteria = GetField(fields, "Microsoft.VSTS.Common.AcceptanceCriteria"),
            State = GetField(fields, "System.State"),
            Type = GetField(fields, "System.WorkItemType"),
            AssignedTo = GetNestedField(fields, "System.AssignedTo", "displayName"),
            Priority = GetField(fields, "Microsoft.VSTS.Common.Priority"),
            Tags = GetField(fields, "System.Tags")
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList()
        };

        // Get attachments
        if (doc.RootElement.TryGetProperty("relations", out var relations))
        {
            foreach (var relation in relations.EnumerateArray())
            {
                var rel = relation.GetProperty("rel").GetString();
                if (rel != "AttachedFile") continue;

                var attUrl = relation.GetProperty("url").GetString() ?? "";
                var attName = "";
                if (relation.TryGetProperty("attributes", out var attrs) &&
                    attrs.TryGetProperty("name", out var nameEl))
                {
                    attName = nameEl.GetString() ?? "";
                }

                var attachment = new TicketAttachment
                {
                    FileName = attName,
                    Url = attUrl,
                    ContentType = GetContentTypeFromFileName(attName)
                };

                // Download image attachments for AI analysis
                if (IsImageFile(attName))
                {
                    try
                    {
                        attachment.Content = await _http.GetByteArrayAsync(attUrl);
                    }
                    catch
                    {
                        // Skip if download fails
                    }
                }

                ticket.Attachments.Add(attachment);
            }
        }

        // Get comments
        try
        {
            var commentsUrl = $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems/{ticketId}/comments?api-version=7.1-preview.4";
            var commentsResponse = await _http.GetAsync(commentsUrl);
            if (commentsResponse.IsSuccessStatusCode)
            {
                var commentsJson = await commentsResponse.Content.ReadAsStringAsync();
                var commentsDoc = JsonDocument.Parse(commentsJson);

                if (commentsDoc.RootElement.TryGetProperty("comments", out var comments))
                {
                    foreach (var comment in comments.EnumerateArray())
                    {
                        ticket.Comments.Add(new TicketComment
                        {
                            Author = comment.TryGetProperty("createdBy", out var createdBy) &&
                                     createdBy.TryGetProperty("displayName", out var displayName)
                                     ? displayName.GetString() ?? "" : "",
                            Text = comment.TryGetProperty("text", out var text)
                                   ? text.GetString() ?? "" : "",
                            CreatedAt = comment.TryGetProperty("createdDate", out var date)
                                        ? date.GetDateTime() : DateTime.MinValue
                        });
                    }
                }
            }
        }
        catch
        {
            // Comments are optional — don't fail the whole request
        }

        return ticket;
    }

    private static string GetField(JsonElement fields, string fieldName)
    {
        return fields.TryGetProperty(fieldName, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : value.ToString()
            : "";
    }

    private static string GetNestedField(JsonElement fields, string fieldName, string nestedKey)
    {
        if (fields.TryGetProperty(fieldName, out var value) &&
            value.ValueKind == JsonValueKind.Object &&
            value.TryGetProperty(nestedKey, out var nested))
        {
            return nested.GetString() ?? "";
        }
        return "";
    }

    private static bool IsImageFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp" or ".bmp";
    }

    private static string GetContentTypeFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}
