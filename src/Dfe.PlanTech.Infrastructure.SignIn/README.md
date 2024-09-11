# Dfe.PlanTech.Infrastructure.SignIn

Project containing code for integrating with DFE Sign-in

## Overview

| File                                                            | Purpose/Functionality                                                                              |
| --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| [DfeSignInSetup.cs](./DfeSignInSetup.cs)                        | Methods to inject services + authentication to the web application                                 |
| [DfeOpenIdConnectEvents](./DfeOpenIdConnectEvents.cs)           | Manages URIs and URLs for login and signout events |
| [UserClaimsExtensions.cs](./Extensions/UserClaimsExtensions.cs) | Extensions for retrieving common values from user roles (currently UserId and Organisation)       |
| [OnUserInformationReceivedEvent.cs](./ConnectEvents/OnUserInformationReceivedEvent.cs) | Records sign in and adds user and organisation claims to principal       |

## How to use

You will need to set the following variables to match the ones set in the DFE Sign-in environment:

| Env variable           | Description                                  | Example                   |
| ---------------------- | -------------------------------------------- | ------------------------- |
| DfeSignIn:Authority    |                                              |                           |
| DfeSignIn:MetadataUrl  |                                              |                           |
| DfeSignIn:ClientId     |                                              |                           |
| DfeSignIn:ClientSecret |                                              |                           |
| DfeSignIn:FrontDoorUrl | The resulting path for the app on front-door | https://dev.plan-tech.com |

These should be set in the `Azure KeyVault` for deployed instances, or `dotnet user-secrets` for local testing.

## Running Locally

With DSi enabled, when running the application locally you need to ensure that the server is running in `https` mode. Failure to do so will result in an error similar to 

```
Exception: Correlation failed.
Unknown location
Exception: An error was encountered while handling the remote login.
Microsoft.AspNetCore.Authentication.RemoteAuthenticationHandler<TOptions>.HandleRequestAsync()
```

The easiest way to do this is through the Kestrel settings, by adding something similar to the following to your `appsettings.json`:

``` 
"Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:16251"
      }
    }
  }
```

**Note: Your `DfeSignIn:FrontDoorUrl` variable should match the URL that the site is running from, and this is currently set to `http://localhost:16251`, so you will need to change this as well**


## Additional Information

For additional information on using, or setup, look at [login.dfe.oidc-dotnetclient](https://github.com/DFE-Digital/login.dfe.oidc-dotnetclient)
