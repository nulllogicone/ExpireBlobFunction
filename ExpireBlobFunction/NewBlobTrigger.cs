using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExpireBlobFunction
{
    public static class NewBlobTrigger
    {
        /// <summary>
        /// Every new blob 
        /// </summary>
        /// <param name="myBlob"></param>
        /// <param name="name"></param>
        /// <param name="toDeleteBlobsTable"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("NewBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("%ContainerName%/{name}", Connection = "")]CloudBlockBlob myBlob,
            string name,
            [Table("ToDeleteBlobs")]CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Blob trigger for blob\n Name:{myBlob.Uri.AbsoluteUri} ");

            // config
            int minutesToLive = int.Parse(Environment.GetEnvironmentVariable("MinutesToLive", EnvironmentVariableTarget.Process));
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(minutesToLive);

            // To Delete Blob Record
            var toDeleteBlobEntity = new ToDeleteBlob()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.GetReverseChronologicalValue(expirationTime),
                RowKey = minutesToLive.ToString(),
                ExpirationTime = expirationTime,
                ContainerName = myBlob.Container.Name,
                BlobName = myBlob.Name
            };
            var insertOperation = TableOperation.InsertOrReplace(toDeleteBlobEntity);
            await toDeleteBlobsTable.ExecuteAsync(insertOperation);
        }
    }

    public class ToDeleteBlob : TableEntity
    {
        public DateTime ExpirationTime { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
    }
}
