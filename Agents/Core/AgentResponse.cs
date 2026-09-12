namespace ai_dev_asst_api.Agents.Core;

public class AgentResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static AgentResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static AgentResponse<T> Fail(string error) => new() { Success = false, Error = error };
}
