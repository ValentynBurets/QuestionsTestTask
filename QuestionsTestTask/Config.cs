using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

public static class Config
{
    public static readonly string SolutionFolderPath = GetSolutionFolderPath();


    private static readonly Lazy<IConfigurationRoot> _configuration =
        new Lazy<IConfigurationRoot>(LoadConfiguration);

    public static IConfigurationRoot Configuration => _configuration.Value;

    public static readonly string FolderPath = Path.Combine(SolutionFolderPath, Configuration["FolderName"] 
        ?? throw new InvalidOperationException("FolderName is not configured"));

    public static readonly string AzureOpenAiEndpoint = Configuration["AzureOpenAI:Endpoint"]
        ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured");

    public static readonly string ApiKey = GetApiKeyAsync().GetAwaiter().GetResult();

    public static readonly string ProjectName = Configuration["AzureOpenAI:ProjectName"]
        ?? throw new InvalidOperationException("AzureQuestionAnswering:ProjectName is not configured");

    public static readonly string DeploymentName = Configuration["AzureOpenAI:DeploymentName"]
        ?? throw new InvalidOperationException("AzureQuestionAnswering:DeploymentName is not configured");

    private static IConfigurationRoot LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(SolutionFolderPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    private static async Task<string> GetApiKeyAsync()
    {
        string? secretName = Configuration["AzureOpenAI:ApiKeySecretName"];
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new InvalidOperationException("AzureOpenAI:ApiKeySecretName is not configured");
        }

        string? kvUri = Configuration["AzureKeyVault:KV_URI"];
        if (string.IsNullOrWhiteSpace(kvUri))
        {
            throw new InvalidOperationException("AzureKeyVault:KV_URI is not configured");
        }

        try
        {
            var clientSecretCredential = new ClientSecretCredential(
                Configuration["AzureAD:TenantId"],
                Configuration["AzureAD:ClientId"],
                Configuration["AzureAD:ClientSecret"]
            );

            var secretClient = new SecretClient(new Uri(kvUri), clientSecretCredential);

            var secretValue = await secretClient.GetSecretAsync(secretName);
            return secretValue.Value.Value;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve API key from Azure Key Vault", ex);
        }
    }

    private static string GetSolutionFolderPath()
    {
        string currentDir = AppContext.BaseDirectory;
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "Program.cs")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        return currentDir ?? throw new InvalidOperationException("Solution file not found.");
    }
}