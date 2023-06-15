# Dfe.PlanTech.Infrastructure.SignIn

Project containing code for integrating with DFE Sign-in

## Overview

| File                                                            | Purpose/Functionality                                                                              |
| --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| [DfeSignInSetup.cs](./DfeSignInSetup.cs)                        | Methods to inject services + authentication to the web application                                 |
| [DfeOpenIdConnectEvents](./DfeOpenIdConnectEvents.cs)           | Contains a method that runs on successful token validation, which pulls in user roles from DFE API |
| [UserClaimsExtensions.cs](./Extensions/UserClaimsExtensions.cs) | Extensions for retrieving common values from user roles (currently UserId, and Organisation)       |

## How to use

You will need to set the following variables to match the ones set in the DFE Sign-in environment:

```
DfeSignIn:Authority
DfeSignIn:MetadataUrl
DfeSignIn:ClientId
DfeSignIn:ClientSecret
```

These should be set in the `Azure KeyVault` for deployed instances, or `dotnet user-secrets` for local testing.

## Additional Information

For additional information on using, or setup, look at [login.dfe.oidc-dotnetclient](https://github.com/DFE-Digital/login.dfe.oidc-dotnetclient)