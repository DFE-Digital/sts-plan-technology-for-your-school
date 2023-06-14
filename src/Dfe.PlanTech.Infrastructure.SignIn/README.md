# Dfe.PlanTech.Infrastructure.SignIn

Project containing code for integrating with DFE Sign-in

## Overview

| File                                                            | Purpose/Functionality                                                                              |
| --------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| [DfeSignInSetup.cs](./DfeSignInSetup.cs)                        | Methods to inject services + authentication to the web application                                 |
| [DfeOpenIdConnectEvents](./DfeOpenIdConnectEvents.cs)           | Contains a method that runs on successful token validation, which pulls in user roles from DFE API |
| [UserClaimsExtensions.cs](./Extensions/UserClaimsExtensions.cs) | Extensions for retrieving common values from user roles (currently UserId, and Organisation)       |
