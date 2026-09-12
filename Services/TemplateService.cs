namespace ai_dev_asst_api.Services;

public class TemplateService : ITemplateService
{
    private readonly IWebHostEnvironment _env;

    public TemplateService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> placeholders)
    {
        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Email", $"{templateName}.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template '{templateName}' not found.");

        var template = await File.ReadAllTextAsync(templatePath);

        foreach (var placeholder in placeholders)
        {
            template = template.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        return template;
    }
}
