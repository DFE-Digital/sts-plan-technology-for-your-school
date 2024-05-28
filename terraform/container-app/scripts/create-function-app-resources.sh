az group create --name $RESOURCE_GROUP --location $LOCATION --tags Product="Plan Technology for your School" Environment="Dev" "Service Offering"="Plan Technology for your School"
az storage account create --name $STORAGE_ACCOUNT_NAME --location $LOCATION --resource-group $RESOURCE_GROUP --sku Standard_LRS --allow-blob-public-access false
az functionapp create --resource-group $RESOURCE_GROUP --name $FUNCTION_APP_NAME --storage-account $STORAGE_ACCOUNT_NAME --flexconsumption-location $LOCATION --runtime dotnet-isolated --runtime-version 8.0 

