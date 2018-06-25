using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace ExpireBlobFunction
{
    public static class NewBlobTrigger
    {
        [FunctionName("NewBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("sample-blobs/{name}", Connection = "")]Stream myBlob,
            string name,
            [Table("blobinsertlog")]CloudTable blobInsertLog,
            [Table("todeleteblobs")]CloudTable toDeleteBlobsTable,
            TraceWriter log)
        {
            log.Info($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            // config
            int timeToLive = 5; // days
            DateTime expirationTime = DateTime.UtcNow.AddDays(timeToLive);


            // Insert Blob Log
            var insertBlobLogEntity = new InsertBlobLog()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.ReverseChronologicalValue,
                RowKey = name,
                ExpirationTime = expirationTime
            };
            var updateOperation = TableOperation.InsertOrReplace(insertBlobLogEntity);
            var result = await blobInsertLog.ExecuteAsync(updateOperation);

            // To Delete Blob 
            var toDeleteBlobEntity = new ToDeleteBlob()
            {
                PartitionKey = OliAzurePack.ChronologicalTime.GetReverseChronologicalValue(expirationTime),
                RowKey = name,
                ExpirationTime = expirationTime
            };
            updateOperation = TableOperation.InsertOrReplace(insertBlobLogEntity);
            result = await toDeleteBlobsTable.ExecuteAsync(updateOperation);

            //return new HttpResponseMessage((HttpStatusCode)result.HttpStatusCode);

        }
    }

    internal class InsertBlobLog : TableEntity
    {
        public DateTime ExpirationTime { get; set; }
    }

    internal class ToDeleteBlob : TableEntity
    {
        public DateTime ExpirationTime { get; set; }

    }
}
