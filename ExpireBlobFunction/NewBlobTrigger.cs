using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExpireBlobFunction
{
    public static class NewBlobTrigger
    {
        [FunctionName("NewBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("sample-blobs/{name}", Connection = "")]CloudBlockBlob myBlob,
            string name,
            [Table("todeleteblobs")]CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Blob trigger for blob\n Name:{myBlob.Uri.AbsoluteUri} ");

            // config
            int timeToLive = 5; // days
            DateTime expirationTime = DateTime.UtcNow.AddDays(timeToLive);

            // To Delete Blob Record
            var toDeleteBlobEntity = new ToDeleteBlob()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.GetReverseChronologicalValue(expirationTime),
                RowKey = name,
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
