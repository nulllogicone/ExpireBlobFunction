using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExpireBlobFunction
{
    public static class RemoveExpiredBlobs
    {
        [FunctionName("RemoveExpiredBlobs")]
        public static async Task Run(
            [TimerTrigger("%DeleteBlobCronExpression%")]TimerInfo myTimer,
            [Table("todeleteblobs")] CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var constr = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            var storageAccount = CloudStorageAccount.Parse(constr);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var deleteOlderThanTicks = OliAzurePack.ChronologicalTime.ReverseChronologicalValue;
            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, deleteOlderThanTicks);
            var query = new TableQuery<ToDeleteBlob>().Where(filter);
            var records = await toDeleteBlobsTable.ExecuteQuerySegmentedAsync(query, null);
            foreach (var record in records)
            {
                var container = blobClient.GetContainerReference(record.ContainerName);
                var blockBlob = container.GetBlockBlobReference(record.BlobName);
                await blockBlob.DeleteIfExistsAsync();

                var deleteRecord = TableOperation.Delete(record);
                await toDeleteBlobsTable.ExecuteAsync(deleteRecord);
                log.Info($"Deleted {record.BlobName}");
            }
        }
    }
}
