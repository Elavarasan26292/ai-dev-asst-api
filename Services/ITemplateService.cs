namespace ai_dev_asst_api.Services;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> placeholders);
}
