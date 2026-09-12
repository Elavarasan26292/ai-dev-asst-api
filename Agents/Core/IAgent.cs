namespace ai_dev_asst_api.Agents.Core;

public interface IAgent<TInput, TOutput>
{
    string Name { get; }
    Task<AgentResponse<TOutput>> ExecuteAsync(AgentContext context, TInput input);
}
