# 0029 - Terraform input variable for SQL password

* **Status**: accepted

## Context and Problem Statement

One of the input variables for our terraform is the admin password for the Azure SQL Server. The concern around this is that
even though we mark the variable as sensitive so it is not shown in plain text in plan/apply output, and it's stored securely
as a github secret it will however show as plain text in the terraform state file. 

## Decision Drivers

* Security by Proxy

## Considered Options

### Rely on Azure security

We rely on Azure security to protect the password. Currently we need to have contributor access to access the TF state in any environment above Dev.
The secret we pass into the TF state cannot be used to login to the SQL server as admin because the server only allows AD authentication. 

### Encrypt the TF State so that it is encrypted at rest
We make changes to the TF state so that it is encrypted at rest. This would mean that the password would not be visible.

## Decision Outcome

We are going to continue with option 1, mainly because we believe the state is secure by proxy, and we don't want to add additional complexity to the TF state at this point.
If we find that we need to add additional security to the state in the future we can revisit this decision.