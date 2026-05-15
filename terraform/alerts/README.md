This manages the monitoring alerts.
The Terraform can be run manually or from workflow infrastructure-deploy-alerts
if new alerts are created directly in Azure, add the relevant terraform as a new tf file, and add the existing resources into an import file, 
which should be called from the workflow the first time running on that environment.

If new variables are needed, add to the variables file.

the tf state is stored in the existing terraform resoure group, storage account and container, but in a separate tf state file called alerts.
