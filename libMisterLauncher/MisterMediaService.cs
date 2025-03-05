using FluentFTP;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Net;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using MongoDB.Bson;

[assembly: InternalsVisibleTo("MisterLauncher.Test")]
namespace libMisterLauncher.Service
{

    public class MediaServiceSettings : IMisterSettings
    {

        const string _moduleName = "MisterMedia";

        public string ModuleName { get { return _moduleName; } }

        public string path { get; set; } = "../data";
       

        public bool isValid()
        {
            return !string.IsNullOrEmpty(path);
        }

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "path",
                    value = path,
                    valueType = "text",
                     description = "Path where media files are stored. Absolute or relative path from execution context",
                    update = DateTime.Now
                }
            };
            foreach (var item in result)
                item.SetId();
            return result;
        }

       

        public void LoadModuleSettings(List<ModuleSetting> moduleSettings)
        {
            foreach (var moduleSetting in moduleSettings)
            {
                switch (moduleSetting.name)
                {
                    case "path":
                        path = moduleSetting.value;
                        break;                   
                }


            }
        }        

    }

    public class MisterMediaService : BaseModule<MediaServiceSettings>
    {

        public MisterMediaService() : base()
        {
        }
        public MisterMediaService (MediaServiceSettings? settings) : base (settings)
        {           
        }

        public override bool CheckConnection()
        {

            try
            {
                
                File.WriteAllText(Path.Combine(_settings.path,"check.txt"), "test");
                File.Delete(Path.Combine(_settings.path, "check.txt"));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }        


        public static Guid GetNewID ()
        {
            return Guid.NewGuid();
        }

        public async Task<DownloadMediaResult> Download (string source, string targetpath, Guid fileid)
        {
            var result = new DownloadMediaResult();
            var localpath = Path.Combine(_settings.path, targetpath);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(source);
                client.DefaultRequestHeaders.Add("User-Agent", "MisterLauncher");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync("");
                response.EnsureSuccessStatusCode();                

                result.httpCode = (int)response.StatusCode;

                if (result.httpCode != 200)
                {
                    return result;
                }

                result.contenttype = response.Content.Headers.GetValues("Content-Type").First();

                
                Directory.CreateDirectory(localpath);
               var match = Regex.Match(result.contenttype, "force-.*name=\"(?<filename>.*?)\"");
                var filename = "";
                if (match.Success)
                {
                    result.filename = match.Groups["filename"].Value;
                    result.extension = filename.Split('.').Last();
                    
                }
                else
                {
                    result.extension = result.contenttype.Split('/').Last();
                    if (result.extension == "svg+xml")
                    {
                        result.extension = "svg";
                    }
                    result.filename = fileid.ToString() + "." + result.extension;
                    
                }
                result.pathStored = Path.Combine(localpath, result.filename);

                using (var fs = new FileStream(result.pathStored, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }
                var f = new FileInfo(result.pathStored);
                result.size = f.Length;
            }

            return result;

        }

        public FileInfo AddBinaryFile(string filepath , byte[] content)
        {
            var localpath = Path.Combine(_settings.path, filepath);
            File.WriteAllBytes(localpath, content);
            return new FileInfo(localpath);
        }

        public FileInfo? CompressRepository(string directoryPath, string filename, bool removeSource = true)
        {
            var localpath = Path.Combine(_settings.path, directoryPath);
            if (!Directory.Exists(localpath))
                return null;
            var destpath = Path.Combine(_settings.path, filename);
            ZipFile.CreateFromDirectory(localpath, destpath);
            if (removeSource)
            {
                Directory.Delete(localpath, true);
            }
            return new FileInfo(destpath);
        }

        public List<FileInfo> GetFiles (string directoryPath, string patern)
        {
            var localpath = Path.Combine(_settings.path, directoryPath);
            if (!Directory.Exists(localpath))
                return new List<FileInfo>();
            return Directory.GetFiles(localpath, patern).Select (x => new FileInfo(x)).ToList();
        }

        public DirectoryInfo CreateDir(string directoryPath, bool force = true)
        {
            var localpath = Path.Combine(_settings.path, directoryPath);
            if (Directory.Exists(localpath))
            {
                if (force)
                    Directory.Delete(localpath, true);
                else
                    return new DirectoryInfo(localpath);
            }
            return Directory.CreateDirectory(localpath);
            
        }

        public void DeleteFile(string filepath)
        {
            var localpath = Path.Combine(_settings.path, filepath);
            if (File.Exists(localpath))
            {
                File.Delete(localpath);
            }
           
        }

        public async Task<byte[]?> Load (string fullpath)
        {
            if (string.IsNullOrEmpty(fullpath)) return null;
            if (!File.Exists(Path.Combine(_settings.path, fullpath))) return null;
            return await File.ReadAllBytesAsync(Path.Combine(_settings.path, fullpath));
        }
    }
}
