# API Technical Documentation

## Purpose

The API is the server-side application for AI Dev Assistant. It owns authentication, authorization, users, roles, permissions, persistence, email delivery, ticket-provider integration, and AI-powered ticket analysis.

This document is the architectural guide for AI-assisted maintenance. It explains responsibilities and flow without duplicating implementation code. Source files remain the final authority when behavior is unclear.

## Maintenance Rule

Update this document whenever a change affects:

- API endpoints or request/response contracts
- Authentication, authorization, roles, or permissions
- Database entities, relationships, seed data, or migrations
- External integrations
- Service or folder responsibilities
- Application startup, configuration, or deployment behavior
- Error handling or important architectural decisions

Keep descriptions focused on behavior and decisions. Do not paste source code into this document.

## Technology

- .NET 8 ASP.NET Core Web API
- Entity Framework Core 8
- MySQL through Pomelo Entity Framework Core provider
- JWT bearer authentication
- BCrypt password hashing
- Swagger/OpenAPI for development API exploration
- Anthropic Claude integration for AI analysis
- Azure DevOps integration for ticket retrieval
- SMTP email delivery

## Runtime Boundaries

The API runs independently from the React client.

- Local API URL: http://localhost:5259
- Swagger UI: http://localhost:5259/swagger
- API route prefix: /api
- Development client origin allowed by CORS: http://localhost:5173

The client calls the API over HTTP. The API never depends on client UI state and must validate authentication and authorization independently for every protected request.

## Startup Flow

1. Application configuration is loaded from appsettings files, environment variables, and .NET user-secrets.
2. When a Key Vault URI is configured, Azure Key Vault is added as a higher-priority configuration provider using `DefaultAzureCredential`.
3. The MySQL database context is registered using the configured connection string.
4. JWT authentication and authorization services are registered.
5. Application services and external providers are registered through dependency injection.
6. CORS, controllers, exception handling, authentication, authorization, and Swagger are configured.
7. Requests are routed to controllers.
8. Swagger is exposed only in the Development environment.

The application does not automatically apply migrations during startup. Database migrations must be applied separately before running against a new database.

## Folder Responsibilities

### Controllers

Controllers define the HTTP API boundary. They validate and receive requests, call application services, and return HTTP responses. Business rules and persistence logic belong in services rather than controllers.

Current controller areas:

- AuthController: login and access-token refresh
- UsersController: user creation, listing, updating, deletion, and password changes
- RolesController: role creation, updates, deletion, listing, and permission listing
- AgentsController: ticket analysis requests

### DTOs

DTOs define the external request and response shapes. They prevent database entities from becoming the public API contract. Changes to DTOs can affect the client and must be coordinated with the client service layer.

### Services

Services contain application use cases and coordinate database operations, validation, hashing, token generation, email, and integration calls.

- AuthService: verifies credentials, creates access tokens, rotates refresh tokens, and maps authenticated users
- UserService: user management and password changes
- RoleService: role management and role-permission relationships
- EmailService: SMTP message delivery
- TemplateService: email-template loading and rendering

Interfaces define service boundaries and make controllers dependent on abstractions.

### Data

AppDbContext defines entity sets, relationships, indexes, composite keys, and seed data.

The main persistence model contains users, roles, permissions, role-permission links, and refresh tokens. Role-permission is a many-to-many relationship represented by a composite key.

### Models

Models represent persisted database entities. Permission IDs are stable GUIDs. Permission names are display labels and must not be used as authorization identifiers.

### Migrations

Entity Framework migrations describe database schema changes and initial seed data. Any entity or relationship change requires a migration review. Do not edit an applied migration to change existing production behavior; add a new migration.

### Filters

PermissionAuthorizeAttribute is an MVC authorization filter. It runs after authentication and before the controller action. It is not middleware.

The filter reads permission claims from the authenticated JWT and compares them with the permission ID required by the endpoint. It returns unauthorized when the request is unauthenticated and forbidden when the authenticated user lacks the required permission.

### Middleware

Middleware handles concerns that apply to the HTTP pipeline. ExceptionHandlingMiddleware converts unhandled exceptions into consistent API error responses. MVC authorization filters remain separate because they apply to controller actions.

### Integrations

Integration abstractions isolate external providers from application services.

- Ticket provider abstraction retrieves ticket data from Azure DevOps.
- AI provider abstraction sends analysis prompts to Claude and receives structured analysis results.
- Email integration sends notification messages through SMTP.

Provider credentials must come from configuration, .NET user-secrets, or Azure Key Vault, never from source code.

## Configuration and Secrets

Local development uses `appsettings.json` plus .NET user-secrets. Production can set `KeyVault__VaultUri` to enable Azure Key Vault. The application uses `DefaultAzureCredential`, which supports managed identity in Azure hosting environments and developer credentials when explicitly used outside production.

Key Vault secret names use `--` for nested configuration sections. For example, `Jwt--Key` supplies `Jwt:Key`, and `ConnectionStrings--DefaultConnection` supplies the database connection string. Key Vault is loaded after the normal configuration providers, so its values override local defaults.

The production hosting identity must be granted permission to read secrets from the vault. The preferred Azure RBAC role is **Key Vault Secrets User**. The vault URI is supplied by the hosting environment and is not stored as a secret inside the vault.

### Agents

Agents represent higher-level AI workflows. The ticket analyzer coordinates ticket input, prompt construction, AI provider calls, and result mapping. Core agent abstractions keep the workflow independent of a specific provider.

## Authentication Flow

1. The client submits credentials to the login endpoint.
2. AuthService loads the active user, role, and role permissions from MySQL.
3. The password is verified against the stored BCrypt hash.
4. The API creates a short-lived JWT access token and a persisted refresh token.
5. The response contains token values, expiry information, and user display data.
6. The client stores the session and sends the access token with protected requests.
7. When the access token expires, the client can submit the access token and refresh token to the refresh endpoint.
8. The API validates the refresh token, revokes the old token, loads the current user permissions, and issues a new token pair.

Access tokens currently expire after 15 minutes. Refresh tokens are persisted and rotated.

## Authorization Flow

Authorization is permission-based rather than name-based.

1. Role permissions are loaded from the database during token generation.
2. Each permission ID is written into the JWT as a Permission claim.
3. Protected controller actions declare the required stable permission ID through the authorization filter.
4. The filter compares the required ID with the authenticated claims.
5. A missing permission produces HTTP 403.

Permission IDs are centralized in the permission-ID definition and must match the database seed or persisted permission records. Permission names can change for display purposes without changing authorization identity.

Because permissions are embedded in JWTs, changing a user role or its permissions does not affect an already-issued access token. The user must refresh the token or log in again.

## Database and Seed Data

The default database is MySQL database AI-ASST on localhost port 3306. The initial migration creates the schema and seeds an Admin role, the initial permissions, their role links, and an initial Admin user.

The database is the source of truth for persisted users, roles, permission relationships, refresh tokens, and ticket-related data when such entities are added.

Apply migrations before first use. Do not rely on application startup to create or update the schema.

## Endpoint Contract Summary

### Authentication

- Login accepts user credentials and returns access and refresh tokens.
- Refresh accepts a refresh-token request and returns a replacement token pair.

### Users

- Create, list, update, and delete operations require the user-access permission.
- Password change is authenticated and restricted to the current user.

### Roles

- Create, update, delete, list, and permission-list operations require the role-management permission.
- Role updates use permission IDs when assigning permissions.

### Agents

- Ticket analysis accepts the analysis request, retrieves ticket data through the configured provider, and sends it through the configured AI provider.

When endpoint contracts change, update the corresponding client service and this document together.

## Error and Security Rules

- Keep secrets out of source control and committed configuration.
- Use DTOs at API boundaries.
- Do not trust user IDs or permissions supplied by the client; derive identity and permissions from validated JWT claims and database records.
- Preserve consistent unauthorized and forbidden behavior.
- Keep external provider failures inside the API error-handling strategy.
- Never log passwords, access tokens, refresh tokens, PATs, SMTP passwords, or AI keys.

## Change Workflow for AI Assistants

Before changing code:

1. Read this document to identify the owning layer.
2. Inspect the relevant controller, service, DTO, model, or client contract.
3. Confirm whether the change affects the database, JWT claims, endpoint shape, or external integration.
4. Make the smallest change within the owning boundary.
5. Update migrations when the persistence model changes.
6. Update this document when architecture or behavior changes.
7. Build and run focused validation for the affected area.

Never solve a client display issue by weakening API authorization. Never place database or provider logic in controllers.

## Known Architectural Limitations

- Provider configuration is currently application-level rather than per user.
- JWT permissions remain unchanged until a token is renewed.
- The API and client use configured local URLs and require matching CORS settings.
- Swagger is intended for development use.
- Production secret loading requires an Azure Key Vault URI and an Azure identity with permission to read secrets.
