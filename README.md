# Expire Blob Function
*Automatically delete blobs after a given time*

> **latest Azure update 12th June 2018**  
> This feature is now available in public preview so you might want to check  out  
> https://azure.microsoft.com/en-us/blog/azure-blob-storage-lifecycle-management-public-preview/

Quite often I use blob storage for temporary images or other files that should be deleted after a given time.
In the beginning I wrote a script that iterates through all blobs to delete those with a last modified time older than a couple of weeks. 
This was hard to maintain and sometimes I forgot to run the script for a while. So I wrote:

#### Two Azure Functions

- Blob trigger that creates an entry with expiration time in Table Storage
- Scheduled trigger that removes old blobs

You can deploy the Functions to your subscription / resource group by clicking the button

[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fnulllogicone%2FExpireBlobFunction%2Fmaster%2FExpireBlobFunctionTemplate%2Fazuredeploy.json)  

### It will
- Create an Azure Function resource
- A consumption hosting plan (pay by usage)
- Add Application Insights for monitoring
- Configure source code deployment from this GitHub repository (master branch)

### Configuration parameters
- **App Name** a unique name for the Azure Function
- **Storage Account Name** the name of an existing storage account
- **Container Name** the blob container that should be monitored for deletion
- **Minutes to live** the live time of blobs before they get deleted (default 30 days = 43200)
- **DeleteBlobCronExpression** a cron expression to schedule the timer trigger (default once a day)

You can always change all app settings after deployment in the Azure portal.

### Remarks
I wrote this project mainly to experiment with the 'Deploy to Azure button' and learn about ARM templates.  
**It is not fully tested and should not be used for critical production scenarios.** Please create issues of fork to contribute.





