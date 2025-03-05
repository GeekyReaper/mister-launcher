using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class RegionInformation
    {
        public string region { get; set; } = "";
        public string value { get; set; } = "";

        public bool isEmpty { get { return string.IsNullOrEmpty(region) && string.IsNullOrEmpty(value); } }
    }

    public class LanguageInformation
    {
        public string language { get; set; } = "";
        public string value { get; set; } = "";

        public bool isEmpty { get { return string.IsNullOrEmpty(language) && string.IsNullOrEmpty(value); } }
    }

    public class JsonRequestResult
    {
        public int HttpCode { get; set; }
        public int responsetime { get; set; }
        
        public JsonNode? json { get; set; }

        public void MoveTo (string nodepath)
        {
            if (json == null)
                return;
            foreach (var nodename in nodepath.Split('.'))
            {
                json = json[nodename];
            }
        }

        public string parsingException { get; set; } = "";
        public bool IsValid ()
        {
            return HttpCode == 200 && json != null;
        }
    }

    public class ScGameType
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public int Parent { get; set; } = 0;

        public bool HasParent { get { return Parent > 0; } }
    }
}
