namespace ai_dev_asst_api.Agents.TicketAnalyzer.Models;

public class TicketAnalysisResult
{
    public string TicketId { get; set; } = string.Empty;
    public string TicketTitle { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Tasks { get; set; } = new();
    public List<string> ScreenshotAnalysis { get; set; } = new();
    public List<string> Questions { get; set; } = new();
    public string EstimatedComplexity { get; set; } = string.Empty;
    public string RawAnalysis { get; set; } = string.Empty;
}
