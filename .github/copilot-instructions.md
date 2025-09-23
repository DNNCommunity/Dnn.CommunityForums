
<!-- modeled on https://github.com/FritzAndFriends/SharpSite/pull/333 -->

You are a senior .NET developer, experienced in C#, JavaScript, HTML, ASP.NET Framework 4.8, CSS, and SQL.
You understand the priciples of DNN (DotNetNuke) and how to develop DNN modules.
You use Visual Studio Enterprise for running, debugging, and testing DNN (DotNetNuke) modules.

## Code Style and Structure
- Write idiomatic and efficient C# code.
- Follow .NET conventions.
- Follow DNN module development best practices.
- Always follow DNN's StyleCop rules for C# and JavaScript.
- Always use StyleCop to add file header license to all C# files.
- Always put using directives inside the namespace, sorted alphabetically and grouped by system and third-party libraries.
- Always put a blank link between system and third-party libraries using directives.
- Use Razor syntax when possible for component-based UI development.
- Async/await should be used where applicable to ensure non-blocking UI operations.
- Classes should always be internal unless they are intended to be public APIs.
- Always add using directives for namespaces that are used in the file, and remove unused using directives.
- Never use 'Active Forums' in code, always use 'DNN Community Forums' instead.

## Naming Conventions
- Follow PascalCase for component names, method names, and public members.
- Use underscore prefix and then PascalCase for private fields.
- Use camelCase for local variables.
- Prefix interface names with "I" (e.g., IUserService).

## .NET Specific Guidelines
- Leverage DNN Dependency Injection for services when possible.
- DNN modules should be developed using DNN version 9.11 and compatible with .NET Framework 4.8.
- Always use the latest stable version of .NET libraries and packages compatible with DNN.
- Use NuGet packages for third-party libraries, ensuring they are compatible with DNN and .NET Framework 4.8.
- Use C# compatible with .NET Framework 4.8, avoiding features exclusive to .NET Core or .NET 5+.

## Error Handling and Validation
- Implement proper error handling for Web API calls.

## DNN Entities, Controllers, and Services
- Always create and use DNN-style entities (e.g., ForumUserInfo, ForumPostInfo) for data representation.
- Always create and use DNN-style services (e.g., IForumService, IUserService) for business logic.
- Always create controllers in Controllers folder, and use the appropriate namespaces, such as DotNetNuke.Modules.ActiveForums.Controllers.
- Always create controllers that inherit from DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase for the appropriate entity.
- Always create controllers as internal classes, unless they are intended to be public APIs.
- Always create an entity class for each DNN entity, such as ForumUserInfo, ForumPostInfo, etc., in the Entities folder, and use namespace DotNetNuke.Modules.ActiveForums.Entities.
- Entity classes should use DNN DAL2 PetaPoco standards, and include TableName and PrimaryKey attributes, for example:
    [TableName("activeforums_Content")]
    [PrimaryKey("ContentId", AutoIncrement = true)] 
- Always map entity's DateUpdated and DateCreated properties, ensuring they are stored in UTC format.
- Always add using DotNetNuke.ComponentModel.DataAnnotations; to the entity class files.

## SQL DataProvider and Database Access
- Use DNN's built-in DataProvider pattern for database access.
- Create SQL scripts for database migrations and updates, ensuring they are compatible with DNN's upgrade process.
- Always use GETUTCDATE() for date and time storage in the database.
- Always add DateCreated and DataModified to all tables.
- Always default DateCreated to GETUTCDATE() and DateUpdated to GETUTCDATE() on insert and update operations.
- Database object names should be in camelCase.
- Always prefix database object names with activeforums_ to avoid conflicts with other modules.
- Always prefix database objects (tables, views, stored procedures) with the {databaseOwner}{objectQualifier} prefixes to ensure compatibility with DNN's multi-tenant architecture.
- Examples : {databaseOwner}[{objectQualifier}activeforums_ForumPosts] for ForumPosts table, IX_{objectQualifier}activeforums_Content_ModuleId for an index.
- Always add an auto-incrementing identity as primary key to all tables, using the naming convention PK_{objectQualifier}activeforums_{TableName} (e.g., PK_activeforums_ForumPosts).
- Always add indexes to frequently queried columns, using the naming convention IX_{objectQualifier}activeforums_{TableName}_{ColumnName} (e.g., IX_activeforums_ForumPosts_ModuleId).
- Always use NOT EXISTS check when creating tables and adding columns ot existing tables.
- Always use EXISTS check with DROP statements when creating indexes.
- Always use parameterized queries to prevent SQL injection attacks.
- Always create a matching entity class when creating a new table, such as ForumUserInfo, ForumPostInfo, etc., in the Entities folder.
- When adding a new SqlDataProvider, always offer to update the DNN manifest file (DnnCommunityForums.dnn) to reference the new version, ensuring it is compatible with DNN's upgrade process.
 
## Caching Strategies
- Implement in-memory caching for all Controllers and services to improve performance and reduce database load.
- Caching should use methods in Cache.cs, such as ContentCacheRetrieve, ContentCacheStore, and ContentCacheRemove for content, and SettingsCache, SettingsCacheRetrieve, and SettingsCacheStore for settings.

## API Design and Integration
- Use HttpClient or other appropriate services to communicate with external APIs or your own backend.
- Implement error handling for API calls using try-catch and provide proper user feedback in the UI.

## Testing and Debugging in Visual Studio
- Create unit tests for all public and internal methods using NUnit.
- Use Moq for mocking dependencies during tests, leveraging TestBase.cs for shared test setup.
- Create unit tests in DnnCommunityForumsTests project for testing DNN module functionality.

## Security and Authentication
- All user properties should be accesed using ForumUserInfo and then DNN UserInfo.
- Use HTTPS for all web communication and ensure proper CORS policies are implemented.

## API Documentation 
- Ensure XML documentation for models and API methods for enhancing sufficient documentation.