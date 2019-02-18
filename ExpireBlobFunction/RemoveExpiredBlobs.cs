using System;
using System.Linq;
using System.Threading.Tasks;
using ExpireBlobFunction.Utils;
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
        /// <summary>
        /// Scheduled trigger calls this Function to delete blobs
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="toDeleteBlobsTable"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("RemoveExpiredBlobs")]
        public static async Task Run(
            [TimerTrigger("%DeleteBlobCronExpression%")]TimerInfo myTimer,
            [Table("ToDeleteBlobs")] CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Timer trigger RemoveExpiredBlobs function executed at: {DateTime.Now}");

            var constr = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            var storageAccount = CloudStorageAccount.Parse(constr);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var deleteOlderThanTicks = ChronologicalTime.ReverseChronologicalValue;
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
