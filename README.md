# ExpireBlobFunction
*Automatically delete blobs after a given time*

Two Azure Functions
- Blob trigger that creates an Table Storage entry with the expiration time
- Scheduled trigger that removes old blobs

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fnulllogicone%2FExpireBlobFunction%2Fmaster%2FExpireBlobFunctionTemplate%2Fazuredeploy.json)

### local.settings.json
```
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<your storage connection string>",
    "AzureWebJobsDashboard": "<your storage connection string>",
    "ContainerName": "sample-blobs",
    "MinutesToLive": 7
  }
}
```
