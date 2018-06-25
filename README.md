# ExpireBlobFunction
*Automatically delete blobs after a given time*

Two Azure Functions
- Blob trigger that creates an Table Storage entry with the expiration time
- Scheduled trigger that removes old blobs

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

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
