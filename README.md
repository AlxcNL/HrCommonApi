# HrCommonApi
Common functionality used by HR Projects. 

Requires the project using this to have at least a configuration file containing the following details:
```json
"Jwt": {
  "Key": "ThisIsASecretKeyThatIsAtLeast32BytesLong12345", // 32 bytes long secret key.
  "Issuer": "Calendify", // Name of the JWT issuer.
  "Audience": "Users", // Target Audience.
  "ExpireMinutes": 60, // Expiration time in minutes.
  "RefreshTokenValidityInDays": 31 // Exiration time in days.
},
"ConnectionStrings": {
  "DBConnection": "User ID=postgres;Password=admin;Host=localhost;Port=5432;Database=Calendify;Pooling=true;" // Any valid connection string.
},
"HrCommonApi": {
  "ConnectionString": "DBConnection", // A reference to the connection string you want to use for the DB context. Usually refers to the one made above.
  "ApiKeyName": "x-api-key", // The key name which the middleware will use too look for an API key in the header.
  "AcceptedApiKeys": [ "6E3370BC-12FF-4692-8C6F-DE6A56AE2874" ], // A whitelist for accepted API keys. The middleware will check the exact permissions in the database.
  "Namespaces": {
    "Models": "Calendify.Database.Models", // The namespace containing all the entity models to automatically load on startup.
    "Profiles": "Calendify.Profiles", // The namespace containing all Automapper profiles to automatically load on startup.
    "Services": "Calendify.Services" // The namespace containing all the services to automatically add to the Dependency Injection on startup.
  }
}
```
