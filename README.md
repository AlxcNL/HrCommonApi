# HrCommonApi
Common functionality used by HR Projects. 

Requires the project using this to have at least a configuration file containing the following details:
```json
"Jwt": {
  "Key": "", // 32 bytes long secret key.
  "Issuer": "", // Name of the JWT issuer.
  "Audience": "", // Target Audience.
  "ExpireMinutes": , // Expiration time in minutes.
  "RefreshTokenValidityInDays":  // Exiration time in days.
},
"ConnectionStrings": {
  "DBConnection": "" // Any valid connection string.
},
"HrCommonApi": {
  "ConnectionString": "DBConnection", // A reference to the connection string you want to use for the DB context. Usually refers to the one made above.
  "ApiKeyName": "x-api-key", // The key name which the middleware will use too look for an API key in the header.
  "AcceptedApiKeys": [ "" ], // A whitelist for accepted API keys. The middleware will check the exact permissions in the database.
  "Namespaces": {
    "Models": "", // The namespace containing all the entity models to automatically load on startup.
    "Profiles": "", // The namespace containing all Automapper profiles to automatically load on startup.
    "Services": "" // The namespace containing all the services to automatically add to the Dependency Injection on startup.
  }
}
```
