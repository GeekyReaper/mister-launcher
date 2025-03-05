using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using static System.Net.Mime.MediaTypeNames;

namespace libMisterLauncher.Entity
{
    public class RemoteServiceCurrentGame
    {
        public string core { get; set; } = "";
        public string system { get; set; } = "";
        public string systemName { get; set; } = "";
        public string game { get; set; } = "";
        public string gameName { get; set; } = "";

        public bool IsArcade()
        {
            return string.IsNullOrEmpty(system) && string.IsNullOrEmpty(systemName);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(core);
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class RemoteServiceMenuItem
    {
        /// <summary>
        /// Filename of item excluding extension(prefers names.txt).
        /// </summary>
        public string name { get; set; } = "";

        /// <summary>
        /// Absolute path to file.
        /// </summary>
        public string path { get; set; } = "";
        /// <summary>
        /// Path to parent folder.
        /// </summary>
        public string parent { get; set; } = "";
        /// <summary>
        /// Full filename of item.
        /// </summary>
        public string filename { get; set; } = "";
        /// <summary>
        /// extension
        /// </summary>
        public string extension { get; set; } = "";
        /// <summary>
        /// Type of file: folder, mra, rbf, mgl, unknown
        /// </summary>
        public string type { get; set; } = "";
        /// <summary>
        /// File modified date. Format: YYYY-MM-DDThh:mm:ss+TZ
        /// </summary>
        public string modified { get; set; } = "";
        /// <summary>
        /// Cores only. Release date of core from filename. Format: YYYY-MM-DDThh:mm:ss+TZ
        /// </summary>
        public string? version { get; set; }
        /// <summary>
        /// Size of file in bytes.
        /// </summary>
        public int size { get; set; }

    }


    public class RemoteServiceMenuListItem
    {
        public List<RemoteServiceMenuItem> items { get; set; } = new List<RemoteServiceMenuItem>();

    }

    public class RemoteServiceScriptResult
    {
        public bool canLaunch { get; set; } = false;
        public List<RemoteServiceScript> scripts { get; set; } = new List<RemoteServiceScript>();
    }

    public class RemoteServiceScript
    {
        public string name { get; set; } = "";
        public string filename { get; set; } = "";
        public string path { get; set; } = "";

        public bool Compare(string value)
        {
            return name == value ||filename == value || path == value;
        }

        public bool isEmpty ()
        {
            return name == "" && filename == "" && path == "";
        }
    }

        public class RemoteServiceSystem
    {
        public string id { get; set; } = "";
        public string name { get; set; } = "";
        public string category { get; set; } = "";

    }

    public class RemoteScvSystemMatch
    {
        public List<string> Names { get; set; } = new List<string>();
        public string Type { get; set; } = "";
        public JsonObject? json { get; set; }
    }

    public class RemoteScreenshotsInfo
    {
        public string game { get; set; } = "";
        public string filename { get; set; } = "";
        public string core { get; set; } = "";
        public DateTime modified { get; set; } = DateTime.MinValue;

    }

}
