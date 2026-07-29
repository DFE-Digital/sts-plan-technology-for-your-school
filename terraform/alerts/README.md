This manages the monitoring alerts.
The Terraform can be run manually or from workflow infrastructure-deploy-alerts

It takes variables from a tfvars file, and a couple which are passed in directly from the workflow.
Unlike the other infrastructure pipelines, this has the file directly in the codebase as none of the variables are actually secret.
If this changes, adjust the flow to match the other infrastructure pipeline and terraform folders by converting the tfvars json to a standard tfvars file,
uploading to the terraform keyvault under a single secret name, add the secret name to github, and compile the tfvars file within the workflow.
Ensure the service principal (s190p-terraform) has the necessary roles on the key vault - key vault secrets reader and Key Vault Crypto Service Encryption User
(these are in the terraform but were not back applied to production; can be added from the cli by logging in as the terraform SP).

if new alerts are created directly in Azure, add the relevant terraform as a new tf file, and add the existing resources into an import file, 
which should be called from the workflow the first time running on that environment.
There is an example - imports/prodimports, and the step to add them is in the pipeline.

If new variables are needed, add to the variables file and either add to the tfvars file or pass in directly from the workflow.

the tf state is stored in the existing terraform resoure group, storage account and container, but in a separate tf state file called alerts.


Testing locally: as with the other workflows, you can run a workflow from your non-main branch by adding it to the 'push' options
in the yaml so it triggers automatically on push. The branch will need adding to the terraform service principal as a Federated Credential under Certificates & secrets
make sure to comment it out again and remove when done.
Terraform can also be run locally directly in git bash for easy plan testing, but it's good to test the whole flow.

note when adding any terraform to add a role, it's the object ID of the service principal (Entra -> Enterprise Apps) NOT the equivalent app registration.

Environments:
Currently enabled for Dev and ProductionUnprotected.

If adding Test/Staging, check the following areas:
  in locals, ensure the tf storage account name matches - these are not consistent.
  check the secret for the tf storage account name - these were missing in prod unprotected.
