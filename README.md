# AI Dev Assistant API

.NET 8 Web API for authentication, user and role management, permission-based authorization, Azure DevOps ticket access, and Claude ticket analysis.

## Prerequisites

- .NET 8 SDK
- MySQL 8.x running on `localhost:3306`
- MySQL database named `AI-ASST`
- Anthropic API key for ticket analysis
- Azure DevOps organization, project, and personal access token
- SMTP credentials if email notifications are required

## Configuration

`appsettings.json` contains placeholder values. Store real or machine-specific values in .NET user-secrets instead of committing them to the repository.

### Local development

Leave `KeyVault:VaultUri` empty locally and use .NET user-secrets for local credentials. The application will not connect to Azure Key Vault unless a vault URI is configured.

From this project directory, run:

```bash
dotnet user-secrets set "Claude:ApiKey" "your-anthropic-api-key"
dotnet user-secrets set "Claude:Model" "claude-sonnet-4-20250514"
dotnet user-secrets set "AzureDevOps:Organization" "your-organization"
dotnet user-secrets set "AzureDevOps:Project" "your-project"
dotnet user-secrets set "AzureDevOps:Pat" "your-personal-access-token"
```

Set the database connection if it differs from the default:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=AI-ASST;User=root;Password=your-password;"
```

Optional SMTP configuration:

```bash
dotnet user-secrets set "Smtp:Username" "your-email@gmail.com"
dotnet user-secrets set "Smtp:Password" "your-gmail-app-password"
dotnet user-secrets set "Smtp:FromEmail" "your-email@gmail.com"
```

Use a strong JWT key in environments outside local development:

```bash
dotnet user-secrets set "Jwt:Key" "your-at-least-32-character-secret"
```

### Production with Azure Key Vault

Set the Key Vault URI as an application setting or environment variable in the hosting environment. The double underscore maps to the nested configuration key:

```text
KeyVault__VaultUri=https://your-vault-name.vault.azure.net/
```

The API uses `DefaultAzureCredential`, so production should use a managed identity or another Azure-supported workload identity. Grant that identity permission to read secrets from the vault, preferably with the Azure RBAC **Key Vault Secrets User** role.

Store configuration values in Key Vault using double dashes in place of configuration colons:

| Key Vault secret                       | Configuration key                     |
| -------------------------------------- | ------------------------------------- |
| `ConnectionStrings--DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `Jwt--Key`                             | `Jwt:Key`                             |
| `Jwt--Issuer`                          | `Jwt:Issuer`                          |
| `Jwt--Audience`                        | `Jwt:Audience`                        |
| `Smtp--Host`                           | `Smtp:Host`                           |
| `Smtp--Port`                           | `Smtp:Port`                           |
| `Smtp--Username`                       | `Smtp:Username`                       |
| `Smtp--Password`                       | `Smtp:Password`                       |
| `Smtp--FromEmail`                      | `Smtp:FromEmail`                      |
| `Smtp--FromName`                       | `Smtp:FromName`                       |
| `AzureDevOps--Organization`            | `AzureDevOps:Organization`            |
| `AzureDevOps--Project`                 | `AzureDevOps:Project`                 |
| `AzureDevOps--Pat`                     | `AzureDevOps:Pat`                     |
| `Claude--ApiKey`                       | `Claude:ApiKey`                       |
| `Claude--Model`                        | `Claude:Model`                        |

Key Vault values override values from `appsettings.json` because the Key Vault provider is added after the default configuration providers. Do not store `KeyVault:VaultUri` itself as a Key Vault secret; provide it through the hosting environment before application startup.

## Database setup

Make sure MySQL is running, then apply the existing Entity Framework Core migrations:

```bash
dotnet tool restore
dotnet ef database update
```

This creates the schema and seeds the Admin role, permissions, and initial Admin user.

## Run the API

From the `ai-dev-asst-api` directory:

```bash
dotnet restore
dotnet build
dotnet run
```

The API runs at:

- API: <http://localhost:5259>
- Swagger UI: <http://localhost:5259/swagger>

The development URL is configured in `Properties/launchSettings.json`.

## Azure DevOps container pipeline

`azure-pipelines.yml` is manual-only. It runs when you select **Run pipeline** in Azure DevOps. It installs .NET 8, restores dependencies, builds the API, runs tests, builds the Docker image, and pushes it to Azure Container Registry.

Before running the pipeline, create an Azure DevOps Docker Registry service connection for the ACR registry and replace the `dockerRegistryServiceConnection` value in `azure-pipelines.yml` with that service connection name. The image is pushed as:

```text
devasst.azurecr.io/ai-dev-asst-api:<Build.BuildId>
devasst.azurecr.io/ai-dev-asst-api:latest
```

The pipeline does not contain database credentials or Key Vault secrets. Those values are loaded by the deployed application at runtime.

## Default login

The initial migration seeds an Admin user:

- Email: `elavarasan261992@gmail.com`
- Password: `saravanan`

Change this password after the first login in any non-development environment.

## Useful commands

```bash
# Run with the Development profile
dotnet run --launch-profile http

# Create a new migration
dotnet ef migrations add MigrationName

# Apply pending migrations
dotnet ef database update

# List migrations
dotnet ef migrations list
```

## API areas

- `AuthController`: login and refresh tokens
- `UsersController`: user management
- `RolesController`: role and permission management
- `AgentsController`: ticket analysis

The API uses JWT authentication. Permission claims contain permission IDs, and controller actions enforce access through the authorization filter in `Filters/PermissionAuthorizeAttribute.cs`.

## Troubleshooting

- **MySQL connection failure:** confirm MySQL is running on port `3306` and that the connection string credentials are correct.
- **Missing `dotnet ef`:** run `dotnet tool restore`, or install the tool with `dotnet tool install --global dotnet-ef`.
- **Swagger does not appear:** run using the Development environment with `dotnet run --launch-profile http`.
- **CORS errors from the client:** the API allows `http://localhost:5173` by default. Keep the client running on that port or update the CORS origin in `Program.cs`.
- **Unauthorized requests after role changes:** log in again or refresh the token. Permissions are included in the JWT when it is generated.
