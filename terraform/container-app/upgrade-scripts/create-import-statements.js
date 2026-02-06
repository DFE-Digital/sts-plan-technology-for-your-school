/**
 * Extracts existing TF resource errors from the TF apply string, and creates import statements for each one
 * @param {string} inputString Terraform apply failure error messages
 * @param {string} varFile Var file to be used for the imports
 * @returns
 */
function generateTerraformImports(inputString, varFile) {
  const commands = [];

  // Regex to capture the resource ID, type, and name across multiple lines
  const regex = /ID "(.*?)" already exists.*?with (.*?)\.(.*?),/gs;

  let match;
  while ((match = regex.exec(inputString)) !== null) {
    const resourceId = match[1]; // The resource ID
    const resourceType = match[2]; // The resource type (e.g., azurerm_private_endpoint)
    const resourceName = match[3]; // The resource name (e.g., database)

    // Construct the command
    const command = `terraform import -var-file=${varFile} '${resourceType}.${resourceName}' '${resourceId}'`;
    commands.push(command);
  }

  return commands.join('\n');
}

// Replace with _all_ of the failures from the TF apply. Ensure first line formatting matches as shown.
const inputString = `
│ Error: A resource with the ID "/subscriptions/abc/resourceGroups/xyz/providers/Microsoft.ServiceName/resourceType/resourcename" already exists - to be managed via Terraform this resource needs to be imported into the State. Please see the resource documentation for "azurerm_private_endpoint" for more information.
│
│   with azurerm_private_endpoint.database,
│   on database.networking.tf line 7, in resource "azurerm_private_endpoint" "database":
│    7: resource "azurerm_private_endpoint" "database" {
│
│ Error: A resource with the ID "/subscriptions/abc/resourceGroups/xyz/providers/Microsoft.ServiceName/resourceType/resourcename" already exists - to be managed via Terraform this resource needs to be imported into the State. Please see the resource documentation for "azurerm_private_endpoint" for more information.
│   with azurerm_private_endpoint.another_database,
│   on another_database.networking.tf line 7, in resource "azurerm_private_endpoint" "another_database":
│    7: resource "azurerm_private_endpoint" "another_database" {
`;

const varFile = '../terraform-stg.tfvars'; //replace with appropriate TF vars file
const result = generateTerraformImports(inputString, varFile);
console.log(result);
