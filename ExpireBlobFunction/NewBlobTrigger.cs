using System;
using System.IO;
using System.Threading.Tasks;
using ExpireBlobFunction.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExpireBlobFunction
{
    public static class NewBlobTrigger
    {
        /// <summary>
        /// Every new blob in the configured %container% of the storage account
        /// triggers this Function that adds a new entry in the ToDeleteBlobs table
        /// </summary>
        /// <param name="newBlob">Uploaded to the storage account container</param>
        /// <param name="name">the unique blob name in the container </param>
        /// <param name="toDeleteBlobsTable">new record <see cref="ToDeleteBlob"/> for every blob when it should be deleted,</param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("NewBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("%ContainerName%/{name}", Connection = "")]CloudBlockBlob newBlob,
            string name,
            [Table("ToDeleteBlobs")]CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Blob trigger for blob\n Name:{newBlob.Uri.AbsoluteUri} ");

            // config
            int minutesToLive = int.Parse(Environment.GetEnvironmentVariable("MinutesToLive", EnvironmentVariableTarget.Process));
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(minutesToLive);

            // To Delete Blob Record
            var toDeleteBlobEntity = new ToDeleteBlob()
            {
                PartitionKey = ChronologicalTime.GetReverseChronologicalValue(expirationTime),
                RowKey = minutesToLive.ToString(),
                ExpirationTime = expirationTime,
                ContainerName = newBlob.Container.Name,
                BlobName = newBlob.Name
            };
            var insertOperation = TableOperation.InsertOrReplace(toDeleteBlobEntity);
            await toDeleteBlobsTable.ExecuteAsync(insertOperation);
        }
    }

    /// <summary>
    /// TableEntity with records ToDeleteBlobs. PartitionKey is <see cref="OliAzurePack.ChronologicalTime.ReverseChronologicalValue"/>
    /// The <see cref="RemoveExpiredBlobs"/> Function will delete blobs older than <see cref="ExpirationTime"/>
    /// </summary>
    public class ToDeleteBlob : TableEntity
    {
        public DateTime ExpirationTime { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
    }
}
