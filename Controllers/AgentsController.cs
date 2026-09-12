using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ai_dev_asst_api.Agents.Core;
using ai_dev_asst_api.Agents.TicketAnalyzer;
using ai_dev_asst_api.Agents.TicketAnalyzer.Models;

namespace ai_dev_asst_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgent<TicketAnalysisRequest, TicketAnalysisResult> _ticketAnalyzer;

    public AgentsController(IAgent<TicketAnalysisRequest, TicketAnalysisResult> ticketAnalyzer)
    {
        _ticketAnalyzer = ticketAnalyzer;
    }

    [HttpPost("analyze-ticket")]
    public async Task<IActionResult> AnalyzeTicket([FromBody] TicketAnalysisRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";

        var context = new AgentContext
        {
            UserId = userId,
            UserEmail = email
        };

        var result = await _ticketAnalyzer.ExecuteAsync(context, request);

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }
}
