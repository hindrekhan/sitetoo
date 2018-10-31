using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace MediaGallery.FileSystem
{
    public class GoogleDriveFileClient : IFileClient
    {
        private readonly IHostingEnvironment _host;
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "Drive API .NET Quickstart";
        UserCredential credential;
        DriveService service;

        public GoogleDriveFileClient(IHostingEnvironment host)
        {
            _host = host;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });


        }

        public void CreateFolder(string parentPath, string folderName)
        {
            var path = Path.Combine(_host.WebRootPath, "gallery", parentPath, folderName);
            //Google.Apis.Drive.v3.Data.File 


            //FilesResource.CreateRequest request = service.Files.Create()

            //Directory.CreateDirectory(path);
        }

        public Stream GetFile(string path)
        {
            FilesResource.GetRequest request = service.Files.Get(path);

            var filePath = Path.Combine(_host.WebRootPath, "gallery", path);
            Google.Apis.Drive.v3.Data.File file = request.Execute();
            Stream stream = service.HttpClient.GetStreamAsync(file.WebViewLink).Result;
            return stream;
        }

        public string GetFileUrl(string path)
        {
            return "~/" + Path.Combine("gallery", path);
        }

        public bool HasFolder(string folderPath)
        {
            var path = Path.Combine(_host.WebRootPath, "gallery", folderPath);

            return Directory.Exists(path);
        }

        public void UploadPhoto(string path, Stream photo)
        {
            path = Path.Combine(_host.WebRootPath, "gallery", path);

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = path,
            };

            FilesResource.CreateMediaUpload request;
            request = service.Files.Create(fileMetadata, photo, "image/jpeg");
            request.Fields = "id";
            request.Upload();

            var file = request.ResponseBody;
            Console.WriteLine("File ID: " + file.Id);
        }
    }
}
