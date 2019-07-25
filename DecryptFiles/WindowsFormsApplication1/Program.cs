using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
//using Microsoft.Azure;
//using Microsoft.Azure.Storage;
using System;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DecryptFile
{
    class Program
    {
        // datatable to read excel data
        static DataTable dataTable = new DataTable();

        // connection string
        private const string StorageAccountName = "ssohrdev";
        private const string StorageAccountKey = "gSnj3arsV8FHVNU1/tELz3vLF1FcCEbdA132VV5sDKEsxAEN0lesfiHjRIzY/xyV1qHEM3exRqwc7Z/U1553xw==";

        public static void Main()
        {

            // CAll ReadExcel
            //ReadExcel();

            // await till file are decrypted
            Decrypt();

        }

        private static void Decrypt()
        {
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            // read and decrypt
            Dictionary<string, string> AHVB_Details = new Dictionary<string, string>();
            AHVB_Details.Add("Pay_Report_NZ_<YYYYMM>.csv", "KL_Nz_01");
            AHVB_Details.Add("Pay_Report_AU_ < System > _<YYYYMM>.csv", "KL_Au_01");
            AHVB_Details.Add("Pay_Report_NG_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_GH_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_AO_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_KE_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_MZ_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_TZ_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_UG_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_ZA_<YYYYMM>.xlsx", "E@fp9yr001");
            AHVB_Details.Add("Pay_Report_GB_CAM_<YYYYMM>.xlsx", "E@fp9yr00l");
            AHVB_Details.Add("Pay_Report_GB_SLB_<YYYYMM>.xlsx", "E@fp9yr00l");
            AHVB_Details.Add("Days Report_IS_<YYYYMM>.xlsx", " SignOff_IS");
            AHVB_Details.Add("Balance_Selloff_Report_EAF_<YYYYMM>.xls", "P@55w0rd");
            AHVB_Details.Add("Balance_Selloff_Report_MEA_<YYYYMM>.xls", "P@55w0rd");
            AHVB_Details.Add("Balance_Selloff_Report_ASA_<YYYYMM>.xls", "P@55w0rd");

            //foreach (DataRow dataRow in dataTable.Rows)
            foreach (KeyValuePair<string, string> ahvb in AHVB_Details)
            {

                string year = DateTime.Now.Year.ToString();

                string month = (DateTime.Now.Month - 1).ToString();

                if (month.Count() == 1)
                {
                    month = "0" + month;
                }

                //string fileName = dataRow["File Name"].ToString();
                //string passWord = dataRow["Pwd"].ToString();

                string fileName = ahvb.Key;
                string passWord = ahvb.Value;
                string fileSlicedName = fileName.Substring(0, fileName.IndexOf("<"));
                //Console.WriteLine("gpg --batch --yes --passphrase " + passWord + " --decrypt-files \"C:\\AHVB_Blob\\" + fileSlicedName + year + month  + "*.gpg\"");
                if (fileSlicedName.Contains("AU"))
                {
                    fileSlicedName = fileSlicedName.Substring(0, fileSlicedName.IndexOf("AU"));

                    process.StandardInput.WriteLine("gpg --batch --yes --passphrase " + passWord + " --decrypt-files \"C:\\Users\\Public\\Documents\\AHVB_Blob\\" + fileSlicedName + "*_" + year + month + "*\"");
                    //string file = "C:\\AHVB_Blob\\" + fileSlicedName + "*_" + year + month + "*";
                }
                else
                {
                    process.StandardInput.WriteLine("gpg --batch --yes --passphrase " + passWord + " --decrypt-files \"C:\\Users\\Public\\Documents\\AHVB_Blob\\" + fileSlicedName + year + month + "*\"");
                    //Console.WriteLine(string.Format("C:\\AHVB_Blob\\" + fileSlicedName + year + month + "*\""));
                }
            }

            process.StandardInput.Flush();
            process.StandardInput.Close();

            //wait till all the process of file decryption is finished.
            process.WaitForExit();

            CallUploadAsync();


        }
        public static void CallUploadAsync()
        {
            if (String.IsNullOrEmpty(StorageAccountName) || String.IsNullOrEmpty(StorageAccountKey))
            {
                throw new InvalidOperationException("One or more account credential strings have not been populated. Please ensure that your Batch and Storage account credentials have been specified.");
            }
            try
            {
                //Console.WriteLine("Sample start: {0}", DateTime.Now);
                //Console.WriteLine();
                //Stopwatch timer = new Stopwatch();
                //timer.Start();

                // Create the blob client, for use in obtaining references to blob storage containers
                CloudBlobClient blobClient = CreateCloudBlobClient(StorageAccountName, StorageAccountKey);

                // Use the blob client to create the input container in Azure Storage 
                const string inputContainerName = "ahvb-decrypted-files";

                CloudBlobContainer container = blobClient.GetContainerReference(inputContainerName);

                SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                    Permissions = SharedAccessBlobPermissions.Read
                };

                // Create the container
                //container.CreateAsync();
                container.CreateIfNotExistsAsync().Wait();


                string[] paths = Directory.GetFiles(@"C:\Users\Public\Documents\AHVB_Blob\", "*");
                foreach (var items in paths)
                {
                    if (Path.GetExtension(items) != ".gpg" && Path.GetExtension(items) != ".pgp")
                    {
                        UploadFileToContainerAsync(blobClient, inputContainerName, items);
                        Console.WriteLine(items);

                    }
                }

            }
            finally
            {
                Console.WriteLine();
                //Console.WriteLine("Sample complete, hit ENTER to exit...");
                //Console.ReadLine();
            }

        }
        public static CloudBlobClient CreateCloudBlobClient(string storageAccountName, string storageAccountKey)
        {
            // Construct the Storage account connection string
            string storageConnectionString =
                $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}";

            // Retrieve the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            // return the blob client

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;

        }

        public static void UploadFileToContainerAsync(CloudBlobClient blobClient, string containerName, string filePath)
        {
            Console.WriteLine("Uploading file {0} to container [{1}]...", filePath, containerName);

            string blobName = Path.GetFileName(filePath);

            filePath = Path.Combine(Environment.CurrentDirectory, filePath);

            // Taking container reference for the second time.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blobData = container.GetBlockBlobReference(blobName);
            blobData.UploadFromFileAsync(filePath).Wait();

            // Set the expiry time and permissions for the blob shared access signature. 
            // In this case, no start time is specified, so the shared access signature 
            // becomes valid immediately

            // Construct the SAS URL for blob
            //string sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
            //string blobSasUri = String.Format("{0}{1}", blobData.Uri, sasBlobToken);


        }

        // Reading passphrase from excel has been deprecated, now the passwords are in dictionary inside the source code.
        static void ReadExcel()
        {
            string Flocation = @"C:\ahvb-details.xlsx";

            var fileName = Flocation;
            var connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\"");

            string sheetNames = "Sheet1$";

            //DataSet dataSet = new DataSet();
            OleDbConnection oleDbConnection = new OleDbConnection(connectionString);

            string sqlQuery = string.Format("SELECT * FROM [{0}]", sheetNames);

            oleDbConnection.Open();
            OleDbDataAdapter oleDbDataAdapter = new OleDbDataAdapter(sqlQuery, oleDbConnection);
            oleDbDataAdapter.Fill(dataTable);
            //dataSet.Tables.Add(dataTable);
            oleDbConnection.Close();


        }


    }
}
