using Anthropic;

namespace ai_dev_asst_api.Integrations.AiProviders;

public class ClaudeProvider : IAiProvider
{
    private readonly AnthropicClient _client;
    private readonly string _model;

    public string ProviderName => "Claude";

    public ClaudeProvider(IConfiguration config)
    {
        var apiKey = config["Claude:ApiKey"]!;
        _model = config["Claude:Model"] ?? "claude-sonnet-4-20250514";
        _client = new AnthropicClient(apiKey);
    }

    public async Task<string> ChatAsync(string systemPrompt, string userMessage)
    {
        var response = await _client.Messages.MessagesPostAsync(
            model: _model,
            messages: new List<InputMessage>
            {
                new()
                {
                    Role = InputMessageRole.User,
                    Content = new List<InputContentBlock>
                    {
                        new(new RequestTextBlock { Text = userMessage })
                    }
                }
            },
            maxTokens: 4096,
            system: systemPrompt
        );

        return ExtractText(response);
    }

    public async Task<string> ChatWithImagesAsync(string systemPrompt, string userMessage, List<AiImageContent> images)
    {
        var contentBlocks = new List<InputContentBlock>();

        foreach (var image in images)
        {
            contentBlocks.Add(new InputContentBlock(new RequestImageBlock
            {
                Source = new Base64ImageSource
                {
                    MediaType = MapMediaType(image.MediaType),
                    Data = image.Data
                }
            }));
        }

        contentBlocks.Add(new InputContentBlock(new RequestTextBlock { Text = userMessage }));

        var response = await _client.Messages.MessagesPostAsync(
            model: _model,
            messages: new List<InputMessage>
            {
                new()
                {
                    Role = InputMessageRole.User,
                    Content = contentBlocks
                }
            },
            maxTokens: 4096,
            system: systemPrompt
        );

        return ExtractText(response);
    }

    private static string ExtractText(Message response)
    {
        foreach (var block in response.Content)
        {
            if (block.IsText)
                return block.Text?.Text ?? "";
        }
        return "";
    }

    private static Base64ImageSourceMediaType MapMediaType(string mediaType)
    {
        return mediaType switch
        {
            "image/png" => Base64ImageSourceMediaType.ImagePng,
            "image/jpeg" => Base64ImageSourceMediaType.ImageJpeg,
            "image/gif" => Base64ImageSourceMediaType.ImageGif,
            "image/webp" => Base64ImageSourceMediaType.ImageWebp,
            _ => Base64ImageSourceMediaType.ImagePng
        };
    }
}
