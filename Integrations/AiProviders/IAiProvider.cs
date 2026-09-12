namespace ai_dev_asst_api.Integrations.AiProviders;

public class AiMessage
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = string.Empty;
    public List<AiImageContent>? Images { get; set; }
}

public class AiImageContent
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string MediaType { get; set; } = "image/png";
}

public interface IAiProvider
{
    string ProviderName { get; }
    Task<string> ChatAsync(string systemPrompt, string userMessage);
    Task<string> ChatWithImagesAsync(string systemPrompt, string userMessage, List<AiImageContent> images);
}
