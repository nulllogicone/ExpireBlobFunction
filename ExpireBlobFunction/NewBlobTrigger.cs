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
            [Table("blobinsertlog")]CloudTable blobInsertLog,
            [Table("todeleteblobs")]CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Blob trigger for blob\n Name:{myBlob.Uri.AbsoluteUri} ");

            // config
            int timeToLive = 5; // days
            DateTime expirationTime = DateTime.UtcNow.AddDays(timeToLive);


            // Insert Blob Log
            var insertBlobLogEntity = new InsertBlobLog()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.ReverseChronologicalValue,
                RowKey = name,
                ExpirationTime = expirationTime,
                AbsoluteUri = myBlob.Uri.AbsoluteUri
            };
            var updateOperation = TableOperation.InsertOrReplace(insertBlobLogEntity);
            var result = await blobInsertLog.ExecuteAsync(updateOperation);

            // To Delete Blob 
            var toDeleteBlobEntity = new ToDeleteBlob()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.GetReverseChronologicalValue(expirationTime),
                RowKey = name,
                ExpirationTime = expirationTime,
                AbsoluteUri = myBlob.Uri.AbsoluteUri
            };
            updateOperation = TableOperation.InsertOrReplace(insertBlobLogEntity);
            result = await toDeleteBlobsTable.ExecuteAsync(updateOperation);

            //return new HttpResponseMessage((HttpStatusCode)result.HttpStatusCode);

        }
    }

    internal class InsertBlobLog : TableEntity
    {
        public DateTime ExpirationTime { get; set; }
        public string AbsoluteUri { get; set; }
    }

    internal class ToDeleteBlob : TableEntity
    {
        public DateTime ExpirationTime { get; set; }
        public string AbsoluteUri { get; set; }

    }
}
