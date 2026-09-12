using ai_dev_asst_api.Integrations.TicketProviders.Models;

namespace ai_dev_asst_api.Integrations.TicketProviders;

public interface ITicketProvider
{
    string ProviderName { get; }
    Task<RawTicket> GetTicketAsync(string ticketId);
}
