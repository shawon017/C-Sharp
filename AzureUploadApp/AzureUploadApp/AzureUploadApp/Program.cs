using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureUploadApp
{
    class Program
    {
   
        public static void Main(string[] args)
        {
            CallUploadAsync();
        }
        public static void CallUploadAsync()
        {

            string connString = ConfigurationManager.ConnectionStrings["AzureStorageAccount"].ConnectionString;
            string localFolder = @"C:\Users\Public\Documents\AHVB_Blob\";
            string destContainer = "ahvb-vba-container";

            //Get a reference to the storage account
            Console.WriteLine(@"Connecting to storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to the container (creating it if necessary)
            Console.WriteLine(@"Getting reference to container...");
            CloudBlobContainer container = blobClient.GetContainerReference(destContainer);

            // Create if container doesn't exists
            container.CreateIfNotExistsAsync().Wait();

       

            // Upload files
            bool deleteFile = false;
            string[] fileEntries = Directory.GetFiles(localFolder, "*.csv");
            foreach (string filePath in fileEntries)
            {
                //string key = DateTime.UtcNow.ToString("yyyy-MM-dd-HH:mm:SS") + "_" + Path.GetFileName(filePath);
                string key = Path.GetFileName(filePath);

                deleteFile = UploadBlob(container, key, filePath, true);
            }

            Console.WriteLine(@"Upload processing complete. Press any key to continue...");

            if (deleteFile)
            {
                //delete files
                EmptyFolder(new DirectoryInfo(@"C:\Users\Public\Documents\AHVB_Blob\"));

            }

            void EmptyFolder(DirectoryInfo directoryInfo)
            {
                foreach (FileInfo file in directoryInfo.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo subFolder in directoryInfo.GetDirectories())
                {
                    EmptyFolder(subFolder);
                }
            }
        }

        public static bool UploadBlob(CloudBlobContainer container, string key, string fileName, bool deleteAfter)
        {
            Console.WriteLine(@"Uploading file to container: key=" + key + " source file=" + fileName);

            //  Get a blob reference to write this file to
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(key);

            // write the file
            using (var files = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                blockBlob.UploadFromStream(files);
            }
            return true;
            //if delete of file is requested, then do that
            //if (deleteAfter)
            //{
            //    string fileSlicedName = fileName.Substring(0, fileName.LastIndexOf('.'));

            //    List<string> ext = new List<string> { ".csv", ".csv.gpg", ".xlsx.gpg", ".xlsx", ".CSV.gpg", ".xls.pgp", ".xls" };
            //    DirectoryInfo directory = new DirectoryInfo(@"C:\AHVB_Blob\");
            //    foreach (var extension in ext)
            //    {

            //        //Console.WriteLine(fileSlicedName + extension);
            //        File.Delete(fileSlicedName + extension);

            //    }
            //}
        }
    }
}
