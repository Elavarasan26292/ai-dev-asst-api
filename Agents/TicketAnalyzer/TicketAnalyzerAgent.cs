using System.Text.Json;
using ai_dev_asst_api.Agents.Core;
using ai_dev_asst_api.Agents.TicketAnalyzer.Models;
using ai_dev_asst_api.Integrations.AiProviders;
using ai_dev_asst_api.Integrations.TicketProviders;

namespace ai_dev_asst_api.Agents.TicketAnalyzer;

public class TicketAnalyzerAgent : IAgent<TicketAnalysisRequest, TicketAnalysisResult>
{
    private readonly ITicketProvider _ticketProvider;
    private readonly IAiProvider _aiProvider;

    public string Name => "TicketAnalyzer";

    public TicketAnalyzerAgent(ITicketProvider ticketProvider, IAiProvider aiProvider)
    {
        _ticketProvider = ticketProvider;
        _aiProvider = aiProvider;
    }

    public async Task<AgentResponse<TicketAnalysisResult>> ExecuteAsync(
        AgentContext context, TicketAnalysisRequest input)
    {
        // Step 1: Fetch ticket from the configured provider
        var ticket = await _ticketProvider.GetTicketAsync(input.TicketId);

        // Step 2: Build the analysis prompt
        var comments = ticket.Comments
            .Select(c => $"{c.Author} ({c.CreatedAt:yyyy-MM-dd}): {c.Text}")
            .ToList();

        var prompt = TicketAnalyzerPrompts.BuildAnalysisPrompt(
            ticket.Title,
            ticket.Description,
            ticket.AcceptanceCriteria,
            ticket.Type,
            ticket.State,
            ticket.Priority,
            ticket.AssignedTo,
            ticket.Tags,
            comments
        );

        // Step 3: Call AI — with images if attachments contain screenshots
        string aiResponse;

        var imageAttachments = ticket.Attachments
            .Where(a => a.Content != null && a.ContentType.StartsWith("image/"))
            .Select(a => new AiImageContent
            {
                Data = a.Content!,
                MediaType = a.ContentType
            })
            .ToList();

        if (imageAttachments.Count > 0)
        {
            var imagePrompt = prompt + $"\n\n{imageAttachments.Count} screenshot(s) are attached. Analyze them and describe what they show in relation to the ticket.";
            aiResponse = await _aiProvider.ChatWithImagesAsync(
                TicketAnalyzerPrompts.SystemPrompt,
                imagePrompt,
                imageAttachments
            );
        }
        else
        {
            aiResponse = await _aiProvider.ChatAsync(
                TicketAnalyzerPrompts.SystemPrompt,
                prompt
            );
        }

        // Step 4: Parse AI response
        var result = ParseAnalysis(aiResponse, ticket);
        return AgentResponse<TicketAnalysisResult>.Ok(result);
    }

    private static TicketAnalysisResult ParseAnalysis(string aiResponse, Integrations.TicketProviders.Models.RawTicket ticket)
    {
        var result = new TicketAnalysisResult
        {
            TicketId = ticket.Id,
            TicketTitle = ticket.Title,
            TicketType = ticket.Type,
            State = ticket.State,
            RawAnalysis = aiResponse
        };

        try
        {
            // Try to extract JSON from the response (AI might wrap it in markdown)
            var jsonStr = aiResponse;
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                jsonStr = aiResponse[jsonStart..(jsonEnd + 1)];
            }

            var parsed = JsonDocument.Parse(jsonStr);
            var root = parsed.RootElement;

            if (root.TryGetProperty("summary", out var summary))
                result.Summary = summary.GetString() ?? "";

            if (root.TryGetProperty("tasks", out var tasks))
                result.Tasks = tasks.EnumerateArray()
                    .Select(t => t.GetString() ?? "").Where(t => t != "").ToList();

            if (root.TryGetProperty("screenshotAnalysis", out var screenshots))
                result.ScreenshotAnalysis = screenshots.EnumerateArray()
                    .Select(s => s.GetString() ?? "").Where(s => s != "").ToList();

            if (root.TryGetProperty("questions", out var questions))
                result.Questions = questions.EnumerateArray()
                    .Select(q => q.GetString() ?? "").Where(q => q != "").ToList();

            if (root.TryGetProperty("estimatedComplexity", out var complexity))
                result.EstimatedComplexity = complexity.GetString() ?? "Medium";
        }
        catch
        {
            // If JSON parsing fails, put the raw response in summary
            result.Summary = aiResponse;
            result.EstimatedComplexity = "Unknown";
        }

        return result;
    }
}
