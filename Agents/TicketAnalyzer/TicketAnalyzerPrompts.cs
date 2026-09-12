namespace ai_dev_asst_api.Agents.TicketAnalyzer;

public static class TicketAnalyzerPrompts
{
    public const string SystemPrompt = @"You are a senior software development analyst. Your job is to analyze development tickets and provide a clear, actionable summary for developers.

You will receive ticket details from a project management tool (Azure DevOps, Jira, etc.). Analyze the ticket thoroughly and provide your response in the following JSON format:

{
  ""summary"": ""A clear 2-3 sentence summary of what needs to be done"",
  ""tasks"": [
    ""Specific technical task 1"",
    ""Specific technical task 2""
  ],
  ""screenshotAnalysis"": [
    ""Description of what screenshot 1 shows and how it relates to the requirement""
  ],
  ""questions"": [
    ""Any ambiguity or missing information that should be clarified""
  ],
  ""estimatedComplexity"": ""Low|Medium|High|Very High""
}

Guidelines:
- Break down the work into specific, actionable development tasks
- If the description contains HTML, parse the meaningful content from it
- If acceptance criteria exists, make sure every criterion maps to at least one task
- If screenshots are provided, describe what they show and how they relate to the requirement
- Flag any ambiguities or missing information as questions
- Estimate complexity based on the scope of changes required
- Respond ONLY with valid JSON, no markdown or extra text";

    public static string BuildAnalysisPrompt(string title, string description, string acceptanceCriteria,
        string type, string state, string priority, string assignedTo, List<string> tags, List<string> comments)
    {
        var prompt = $@"Analyze the following development ticket:

**Ticket Title:** {title}
**Type:** {type}
**State:** {state}
**Priority:** {priority}
**Assigned To:** {assignedTo}
**Tags:** {(tags.Count > 0 ? string.Join(", ", tags) : "None")}

**Description:**
{(string.IsNullOrWhiteSpace(description) ? "No description provided." : description)}

**Acceptance Criteria:**
{(string.IsNullOrWhiteSpace(acceptanceCriteria) ? "No acceptance criteria provided." : acceptanceCriteria)}";

        if (comments.Count > 0)
        {
            prompt += "\n\n**Comments/Discussion:**\n" + string.Join("\n---\n", comments);
        }

        prompt += "\n\nProvide your analysis as JSON.";

        return prompt;
    }
}
