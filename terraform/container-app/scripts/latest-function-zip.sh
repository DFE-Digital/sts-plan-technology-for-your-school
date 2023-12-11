#!/bin/bash

storage_account_name="$1"
container_name="function-releases"

latest_blob=$(az storage blob list --account-name $storage_account_name --container-name $container_name --query "[?contains(name,'.zip')].{Name:name,LastModified:properties.lastModified}" --output json | jq -r 'max_by(.LastModified).Name')

echo {\"zip\":\""$latest_blob"\"}