
using System;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Threading.Tasks;

namespace BatchDotNetUpload
{
    class Program
    {
        private const string StorageAccountName = "ssohrdev";
        private const string StorageAccountKey = "gSnj3arsV8FHVNU1/tELz3vLF1FcCEbdA132VV5sDKEsxAEN0lesfiHjRIzY/xyV1qHEM3exRqwc7Z/U1553xw==";

        static void Main()
        {
            //Console.WriteLine("Hello World!");
            //var containerName = "testcontainerherbi";

            //string storageConnection = CloudConfigurationManager.GetSetting("BlobStorageConnectionString"); CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnection); CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();



            //CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference(containerName); CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference("uploadedfilename.ext");



            //MemoryStream memStream = new MemoryStream();

            //blockBlob.DownloadToStream(memStream);
            //HttpContext.Current.Response.ContentType = blockBlob.Properties.ContentType.ToString();
            //HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; filename=" + blockBlob.ToString());

            //HttpContext.Current.Response.AddHeader("Content-Length", blockBlob.Properties.Length.ToString());
            //HttpContext.Current.Response.BinaryWrite(memStream.ToArray()); HttpContext.Current.Response.Flush();
            //HttpContext.Current.Response.Close();


            CloudStorageAccount stroageAccount = new CloudStorageAccount(
                new StorageCredentials(StorageAccountName, StorageAccountKey), true);

            CloudBlobClient blobClient = stroageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("ahvb-vba");

            var blobs = container.ListBlobs();
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddMonths(15),
                Permissions = SharedAccessBlobPermissions.Read
            };

            DownloadBlobs(blobs);

            Console.WriteLine("Completed");


        }

        private static void DownloadBlobs(IEnumerable<IListBlobItem> blobs)
        {
            string root = @"C:\Users\Public\Documents\AHVB_Blob\";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            //else
            //{
            //    DeleteDirectory(root);
            //    Directory.CreateDirectory(root);
                
            //}
            foreach (var blob in blobs)
            {
                
                if (blob is CloudBlobDirectory blobDirectory)
                {
                    Directory.CreateDirectory(root + blobDirectory.Prefix);
                    Console.WriteLine("Create Directory " + blobDirectory.Prefix);
                    DownloadBlobs(blobDirectory.ListBlobs());
                }
                else if (blob is CloudBlockBlob blockBlob)
                {
                          
                    blockBlob.DownloadToFile(root + blockBlob.Name, FileMode.Create);
                    Console.WriteLine(root + blockBlob.Name);

                }
            }
        }
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}
