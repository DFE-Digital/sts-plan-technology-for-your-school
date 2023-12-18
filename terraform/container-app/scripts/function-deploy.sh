#!/bin/bash

if [ $# -lt 1 ]; then
    echo "Usage: $0 <function_app_name>"
    exit 1
fi

function_app_name="$1"

target_folder="../../src/Dfe.PlanTech.AzureFunctions"

if [ -d "$target_folder" ]; then
    cd "$target_folder"
    func azure functionapp publish "$function_app_name"
    echo "Function App published successfully."
else
    echo "Error: The specified target folder does not exist."
fi
