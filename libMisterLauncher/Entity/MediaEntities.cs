using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class MediaDb
    {
        public string _id { get; set; }
        public string contenttype { get; set; }
        public string extension { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public string path { get; set; }
        public string source { get; set; }
        /// <summary>
        /// Deprecated
        /// </summary>
        public string url { get; set; }
        public string type { get; set; }

        public string targetpath {get;set;}
        public string filename { get; set; }

        public long size { get; set; } = 0;



        public BsonDocument? infos { get; set; }

        public bool IsDownloaded()
        {
            return !string.IsNullOrEmpty(filename);
        }

        /// <summary>
        /// Retrocompatibilité avec nodered
        /// </summary>
        /// <returns></returns>
        public string checktargetPath()
        {
            if(string.IsNullOrEmpty(targetpath))
            {
                if (path.StartsWith("target:"))
                {
                    targetpath = path.Substring(7);                    
                }
                else
                {
                    // / multimedia / Console / NES / 10 - Yard Fight / b5bce503 - c8bc - 491b - a85a - 2f55f97247a6.png
                    var t = path.Split('/').ToList();                    
                    t.RemoveAt(t.Count - 1);
                    targetpath = string.Join('/', t).Replace("/multimedia/", "");

                }
            }

            return targetpath;
        }   

        public Guid GetGuid ()
        {
            return Guid.Parse(_id);
        }

        public string GetFullpath()
        {
            return Path.Combine(targetpath, filename); 
        }




    }

    public class MediaRequest
    {
        public string format { get; set; }
        public string url { get; set; }
        public string targetpath { get; set; }

        public string type { get; set; }

        public BsonDocument? infos { get; set; } = null;

        public bool downloadondemand { get; set; } = true;

    }

    public class DownloadMediaResult
    {
        public int httpCode { get; set; } = 0;
        public string pathStored { get; set; } = string.Empty;

        public string filename { get; set; } = string.Empty;
        public string contenttype { get; set; } = string.Empty;
        public string extension { get; set; } = string.Empty;

        public long size { get; set; } = 0;

        public bool IsSucceed { get { return httpCode == 200; } }
    }

    public class MediaHttp
    {
        public string contentType { get; set; } = "";
        public byte[]? content { get; set; }

        public string filename { get; set; } = "";

        public bool isExists { get { return content != null; } }
    }
    
}
