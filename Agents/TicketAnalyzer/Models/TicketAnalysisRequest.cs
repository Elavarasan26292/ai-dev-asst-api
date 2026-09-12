using System.ComponentModel.DataAnnotations;

namespace ai_dev_asst_api.Agents.TicketAnalyzer.Models;

public class TicketAnalysisRequest
{
    [Required(ErrorMessage = "Ticket ID is required.")]
    public string TicketId { get; set; } = string.Empty;
}
