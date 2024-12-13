# Hogeschool Rotterdam Common API Library (HrCommonApi)

## Overview

The **HrCommonApi** library is designed to serve as a foundational framework for kickstarting projects at Hogeschool Rotterdam. It aims to streamline the development process by providing essential components such as Controllers, Services, Data Entities, JWT authentication, and API Key management. This library is currently utilized by two projects:    
- **CargoHub** for Software Construction (2024-2025)    
- **Calendify** for Web Development (2024-2025)  

While the library offers a robust set of features, it may be considered somewhat over-engineered for second-year school projects.  

## Features  
- **Controllers**: Simplified creation and management of API endpoints.  
  - **CoreController**: A solid base class for common API controllers, including request and response mapping, and a common handler for the flow.  
- **Services**: Organized business logic with Dependency Injection support.  
  - **CoreService**: A solid base class for common CRUD actions on a DataContext, utilizing a `ServiceResult` to communicate the results of service actions.  
- **Data Entities**: Streamlined handling of data models.  
  - **BaseEntity**: The base for all DataContext entities, providing a GUID called `Id`, a `DateTime` for `CreatedAt`, and a `DateTime` for `UpdatedAt`. The CoreService uses these fields, eliminating the need for custom code to fill or update them.  
- **JWT Authentication**: Secure user authentication using JSON Web Tokens.  
- **API Key Management**: Control access to your API using API keys. 

## Getting Started  
  
There are two ways to start using the **HrCommonApi** library:  
  
1. **Add as Project Reference**:  
   - Clone or download this repository and add it as a project reference in your solution.  
  
2. **Use NuGet Package**:  
   - Include the library as a reference via NuGet. You can find the package at [HrCommonApi NuGet Package](https://www.nuget.org/packages/HrCommonApi/).  

### Create the configuration section
Create a configuration file (e.g., `appsettings.json`) or modify it with the required sections. You will add more to this section when you make choices regarding the authentication.

### Choosing Authentication/Authorization
After setting up the configuration section you will want to decide on what kind of authentication/authorization you want to use, if any at all. The current project offers a kind of basic Jwt authentication, and a basic Api Key authentication. You can also use both at the same time if you want that. This project uses Swagger & Swashbuckle, and provides methods of providing tokens for that as well.

#### Jwt Authentication
If you want to use Jwt Authentication you will need to include the `JwtAuthorization` configuration section in your configuration file. More on this can be found in Configuration section below.

To bootstrap for Jwt usage, inside your `Program.cs` you need to include `builder.Services.AddHrCommonJwtApiServices<TDataContext, TUser>(builder.Configuration)` in the builder, and `app.AddHrCommonJwtApiMiddleware<TUser>(builder.Configuration)` in the app. 
- The type parameter `TDataContext` needs to be the `DataContext` implementation for your project.
  - Inside your Datacontext you need to override `OnModelCreating(ModelBuilder modelBuilder)` and have it call `modelBuilder.AddEntityModelsWithJwt<TUser>(configuration)`.
- The type parameter `TUser` needs to be the `User` implementation for your project.

You can just provide `User` if your use case does not require extra fields on the `User` model. The standard case has a `Role` in the claims and you can get the userId from the claims that you can check against the backend for additional permissions.

#### ApiKey Authentication
If you want to use Api Key Authentication you will need to include the `ApiKeyAuthorization` configuration section in your configuration file. More on this can be found in Configuration section below.

To bootstrap for Api Key usage, inside your `Program.cs` you need to include `builder.Services.AddHrCommonKeyApiServices<TDataContext, TApiKey>(builder.Configuration)` in the builder, and `app.AddHrCommonKeyApiMiddleware<TApiKey>(builder.Configuration)` in the app. 
- The type parameter `TDataContext` needs to be the `DataContext` implementation for your project. 
  - Inside your Datacontext you need to override `OnModelCreating(ModelBuilder modelBuilder)` and have it call `modelBuilder.AddEntityModelsWithKey<TApiKey>(configuration)`.
- The type parameter `TApiKey` needs to be the `ApiKey` implementation for your project.

You can just provide `ApiKey` if your use case does not require extra fields on the `ApiKey` model. In most cases the permissions can be dealt with in the database, as it maps them directly to your claims.

#### Mixed Authentication
You can choose to use both types of authentication as well by using `builder.Services.AddHrCommonApiServices<HrDataContext, TUser, TApiKey>(builder.Configuration)`, and `app.AddHrCommonApiMiddleware<TUser, TApiKey>(builder.Configuration)`. You follow the same instructions as the individual cases for the type parameters except that in your DataContext you now use `modelBuilder.AddEntityModels<TUser, TApiKey>(configuration)`. When using both at the same time your Jwt identity is supplemented by the claims from the Api Key.

#### No Authentication
If you do not wish to use any of the provided Authentication features simply use `builder.Services.AddHrCommonApiServices<HrDataContext>(builder.Configuration)`, and `app.AddHrCommonApiMiddleware(builder.Configuration)`. Inside your Datacontext you need to override `OnModelCreating(ModelBuilder modelBuilder)` and have it call `modelBuilder.AddEntityModels(configuration)`. You do not need to include any of the extra configuration sections.

#### Example for Jwt
**Example `Program.cs` for Jwt Authentication  (Replace `TDataContext`, `TUser`):**
```csharp
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // This bit can vary based on your choices
        builder.Services.AddHrCommonJwtApiServices<TDataContext, TUser>(builder.Configuration); 

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // This bit can vary based on your choices
        app.AddHrCommonJwtApiMiddleware<TUser>(builder.Configuration);

        app.MapControllers();

        app.Run();
    }
}
```

**Example `HrDataContext.cs` for Jwt Authentication (Replace `TUser`):**
```csharp
public class HrDataContext(DbContextOptions<HrDataContext> options, IConfiguration configuration) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.AddEntityModelsWithJwt<TUser>(configuration);
}
```

## Configuration  
To use the HrCommonApi library, your project must include a configuration file (e.g., `appsettings.json`). It contains the following options:

- **CorsAllowOrigins** (_Required_): Put all frontend origins in that are allowed to connect.
- **ConnectionString** (_Required_): Reference to the connection string used for the database context.
- **Namespaces** (_Required_):
  - **Models** (_Required_): Namespace containing all entity models to be automatically loaded on startup.
  - **Profiles** (_Required_): Namespace containing all AutoMapper profiles to be automatically loaded on startup.
  - **Services** (_Required_): Namespace containing all services to be automatically added to Dependency Injection on startup.
- **JwtAuthorization** (_Only for Jwt or mixed_):
  - **Key**: A 32-byte secret key used for signing tokens.
  - **Issuer**: Name of the JWT issuer.
  - **Audience**: Target audience for the tokens.
  - **TokenExpirationMinutes**: Duration in minutes for which the token is valid (default is 60 minutes).
  - **RefreshExpirationInMinutes**: Duration in minutes for the refresh token's validity (default is 31 days).
- **ApiKeyAuthorization** (_Only for Api Key or mixed_):
  - **ApiKeyName**: The name of the header that will contain the API key (e.g., 'x-api-key').
  - **AcceptedApiKeys**: An array of accepted API keys (e.g., ["6E3370BC-12FF-4692-8C6F-DE6A56AE2874"]).

### Example:
```json
"HrCommonApi": {
  "CorsAllowOrigins": [ "http://localhost:5000", "http://localhost:3000" ],
  "ConnectionString": "DBConnection", // A reference to the connection string you want to use for the DB context.
  "Namespaces": {
    "Models": "", // The namespace containing all the entity models to automatically load on startup.
    "Profiles": "", // The namespace containing all Automapper profiles to automatically load on startup.
    "Services": "" // The namespace containing all the services to automatically add to the Dependency Injection on startup.
  },
  "JwtAuthorization": {
    "Jwt": {
      "Key": "", // 32 bytes long secret key.
      "Issuer": "", // Name of the JWT issuer.
      "Audience": "", // Target Audience.
      "TokenExpirationMinutes": 60, // Expiration time in minutes. (1 hour)
      "RefreshExpirationInMinutes": 44640 // Expiration time in minutes. (31 days)
    }
  },
  "ApiKeyAuthorization": {
    "ApiKeyName": "x-api-key", // For example 'x-api-key'
    "AcceptedApiKeys": [ "" ] // Contains an array of string, example [ '6E3370BC-12FF-4692-8C6F-DE6A56AE2874' ]
  }
}
```
 
## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve the library.

## License

The project is licensed under the [GPL-3.0 License](https://www.gnu.org/licenses/gpl-3.0.en.html). See the [LICENSE](https://github.com/Tukurai/HrCommonApi/tree/main?tab=GPL-3.0-1-ov-file) file for details.
