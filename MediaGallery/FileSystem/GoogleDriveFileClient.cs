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
        static string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveFile };
        static string ApplicationName = "abc1";
        DriveService service;

        public GoogleDriveFileClient(IHostingEnvironment host)
        {
            _host = host;
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "hindrek.hannus@gmail.com",
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
            var files = GetFiles();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();
        }

        public Stream GetFile(string path)
        {
            var stream = new MemoryStream();
            var file = FindFile(path);

            if (file == null)
            {
                //path on alati empty ja ei oska parandada
                return stream;
            }

            try
            {
                var request = service.Files.Get(file.Id);
                request.Download(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

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
            var body = new Google.Apis.Drive.v3.Data.File();
            body.Name = Path.GetFileName(path);

            try
            {
                FilesResource.CreateMediaUpload request = service.Files.Create(body, photo, "image/jpeg");
                var y = request.Upload();
                var x = request.ResponseBody;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        IList<Google.Apis.Drive.v3.Data.File> GetFiles()
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.PageSize = 100;

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;

            return files;
        }

        Google.Apis.Drive.v3.Data.File FindFile(string path)
        {
            Google.Apis.Drive.v3.Data.File file = null;
            var files = GetFiles();

            foreach (var f in files)
            {
                if (f.Name == Path.GetFileName(path))
                {
                    file = f;
                    break;
                }
            }

            return file;
        }

    }
}
