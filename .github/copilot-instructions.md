# Copilot Instructions

## General Guidelines
- You are a senior .NET developer, experienced in C#, JavaScript, HTML, ASP.NET Framework 4.7.2/4.8, CSS, and SQL.
- You understand the principles of DNN (DotNetNuke) and how to develop DNN modules.
- You use Visual Studio Enterprise for running, debugging, and testing DNN (DotNetNuke) modules.

## Code Style and Structure
- Write idiomatic and efficient C# code.
- Follow .NET conventions.
- Follow DNN module development best practices.
- Follow DNN's StyleCop rules for C# and JavaScript.
- Ensure StyleCop-compliant file headers are present on all C# files.
- Keep only required usings, sorted and grouped as System vs non-System.
- Insert a blank line between System and non-System using directives.
- Use Razor in supported views/components; follow existing module UI patterns otherwise.
- Use async/await where applicable to ensure non-blocking operations.
- Classes should be internal unless they are intended to be public APIs.
- Do not use the legacy product name ("Active Forums") in user-facing text; use "DNN Community Forums" instead. Keep existing namespace names unchanged.
- Use JavaScript solutions without jQuery when possible.

## Naming Conventions
- Follow PascalCase for component names, method names, and public members.
- Use underscore prefix and then PascalCase for private fields.
- Use camelCase for local variables.
- Prefix interface names with "I" (for example, IUserService).
- Prefer explicit qualification for DotNetNuke.Modules.ActiveForums types when needed for clarity or ambiguity; otherwise use normal using directives.

## .NET Specific Guidelines
- Leverage DNN Dependency Injection for services when possible.
- DNN modules should be developed using DNN version 9.11 and compatible with .NET Framework 4.7.2/4.8.
- Use the latest stable version of libraries and packages compatible with DNN and .NET Framework 4.7.2/4.8.
- Use NuGet packages for third-party libraries, ensuring compatibility with DNN and .NET Framework 4.7.2/4.8.
- Use C# features compatible with .NET Framework 4.7.2/4.8; avoid features exclusive to .NET Core or .NET 5+.

## Error Handling and Validation
- Implement proper error handling for Web API calls.
- Use RESX localization resources for validation and user-facing messages instead of hardcoded strings.

## DNN Entities, Controllers, and Services
- Create and use DNN-style entities (for example, ForumUserInfo, ForumPostInfo) for data representation.
- Create and use DNN-style services (for example, IForumService, IUserService) for business logic.
- Create and use DNN-style dependency injection for services, leveraging DNN's built-in DI container.
- Create controllers in the Controllers folder and use appropriate namespaces such as DotNetNuke.Modules.ActiveForums.Controllers.
- Create controllers that inherit from DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase for the appropriate entity.
- Create controllers as internal classes unless they are intended to be public APIs.
- Create an entity class for each new entity in the Entities folder, using namespace DotNetNuke.Modules.ActiveForums.Entities.
- Use DNN DAL2 PetaPoco standards for entity classes and include TableName and PrimaryKey attributes, for example:
  [TableName("activeforums_Content")]
  [PrimaryKey("ContentId", AutoIncrement = true)]
- Map DateCreated and DateUpdated properties on entities and store them in UTC format.
- Add using DotNetNuke.ComponentModel.DataAnnotations; to entity class files.
- Do not apply service locator/.Instance DI registration to controllers that require runtime module-specific constructor parameters (for example, LikeController with portalId/moduleId). Instantiate those explicitly with runtime values.

## SQL DataProvider and Database Access
- Use DNN's built-in DataProvider pattern for database access.
- Create SQL scripts for database migrations and updates, ensuring compatibility with DNN's upgrade process.
- Use GETUTCDATE() for date/time storage in the database.
- Add DateCreated and DateUpdated to all tables.
- Default DateCreated to GETUTCDATE(); set DateUpdated to GETUTCDATE() on insert and update operations.
- Prefix database objects (tables, views, stored procedures) with {databaseOwner}{objectQualifier}activeforums_ for multi-tenant compatibility and conflict avoidance.
- Use PascalCase names after the activeforums_ prefix for table/object names (for example, activeforums_ForumPosts).
- Use naming conventions:
  - Primary keys: PK_{objectQualifier}activeforums_{TableName}
  - Indexes: IX_{objectQualifier}activeforums_{TableName}_{ColumnName}
- Examples: {databaseOwner}[{objectQualifier}activeforums_ForumPosts], IX_{objectQualifier}activeforums_Content_ModuleId.
- Use NOT EXISTS checks when creating tables and adding columns.
- Use EXISTS checks with DROP statements when dropping/recreating indexes.
- Use parameterized queries to prevent SQL injection.
- Create a matching entity class when creating a new table.
- When adding a new SqlDataProvider, offer to update the DNN manifest file (DnnCommunityForums.dnn) to reference the new version.

## Caching Strategies
- Implement in-memory caching for controllers and services to improve performance and reduce database load.
- Use methods in Cache.cs, such as ContentCacheRetrieve, ContentCacheStore, ContentCacheRemove, SettingsCache, SettingsCacheRetrieve, and SettingsCacheStore.

## API Design and Integration
- Use HttpClient or other appropriate services to communicate with external APIs or backend services.
- Implement error handling for API calls with try-catch and provide proper user feedback.

## Testing and Debugging in Visual Studio
- Create unit tests for all new or changed public and internal business-logic methods using NUnit.
- Use Moq for mocking dependencies during tests, leveraging TestBase.cs for shared test setup.
- Create unit tests in DnnCommunityForumsTests for DNN module functionality.
- Tests should inherit from TestBase to get DI for DNN objects.
- Use Assert.That() style for NUnit alignment.
- Use TestCase attributes where practical to cover multiple scenarios.

## Security and Authentication
- Access user properties using ForumUserInfo and then DNN UserInfo.
- Use HTTPS for all web communication and ensure proper CORS policies are implemented.

## API Documentation
- Ensure XML documentation for models and API methods.

## Commit Messages
- Use subject line format "TYPE: description".
- Use types: FIX, ENH, TASK, DOC, TEST.
- Keep subject lines under 50 characters where possible.
- Use imperative mood (for example, "Add feature" not "Added feature").
- Reference issue numbers with # prefix and link to the issue when applicable.

## Pull Requests
- Use .github/PULL_REQUEST_TEMPLATE.md.
- Use subject line format "TYPE: description".
- Use types: FIX, ENH, TASK, DOC, TEST.
- Keep subject lines under 50 characters where possible.
- Use imperative mood.
- Reference issue numbers with # prefix and link to the issue when applicable.

## Cross-repo references for `DotNetNuke`

When code references the DotNetNuke namespace and required types/source files are not present in this workspace, consult:

1. Primary external repository:
   - URL: https://github.com/dnnsoftware/Dnn.Platform

2. Resolution rules:
   - Prefer files in the current workspace first.
   - If a required DotNetNuke type or file is not found locally, use get_file to fetch matching files from the repository above.
   - If multiple candidates exist, prefer the file whose namespace/public surface matches requested symbols and targets closest .NET Framework compatibility (4.7.2/4.8).
   - Prefer the latest stable commit on the default branch unless a specific tag/commit is provided.

3. How to fetch and reference:
   - Use get_file with repository-relative paths when possible.
   - Treat fetched files as read-only references to clarify API shape/behavior.
   - Do not copy large third-party source blocks without checking license/project policy.
   - If adaptation/copy is required, add an appropriate file header/license comment consistent with repository guidelines.

4. Ordering and additional repositories:
   - If additional repositories are needed, list them below in priority order with URLs and preferred subpaths.

5. Fallback behavior and errors:
   - If external repo access fails, note the missing symbol and request the file or repository path from the user.
   - If API compatibility or licensing is unclear, ask for clarification before copying code.
