using Amazon.Runtime.SharedInterfaces.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class Rom
    {
        public string _id { get; set; } = "";      
        public string fullpath { get; set; } = "";
        public string systemCategory { get; set; } = "";        
        public bool isMatch { get; set; } = false;
        public long size { get; set; } = 0;
        public string extension { get; set; } = "";

        public bool official { get; set; } = true;
        public string name { get; set; } = "";
        public string fullname { get; set; } = "";
        public string systemid { get; set; } = "";
        public string version { get; set; } = "";
        public string region { get; set; } = "";
        public string language { get; set; } = "";
        public string supporttype { get; set; } = "";

        public string core { get; set; } = "";
        public MraInfo? mraInfo { get; set; } = null;
        public DateTime date { get; set; } = DateTime.MinValue;
        public DateTime lastscandate { get; set; } = DateTime.Now;
        public DateTime firstscandate { get; set; } = DateTime.Now;

        public string parsingexception { get; set; } = "";
        public int responsetime { get; set; } = 0;

        public string checksum_md5 { get; set; }
        public string checksum_crc { get; set; }

        public int scrapperResult { get; set; } = 0;

        public void SetId(string value = "")
        {
            if (string.IsNullOrEmpty(value))
            {
                value = string.Format("{0}-{1}", systemid=="" ? "Arcade" : systemid, name) + (official ? "" : "-unofficial");
            }

            _id = Service.Tools.ReplaceSpecialCaracteres(value);           
        }

        public void SetFileName (string fullpath)
        {
            this.fullpath = fullpath;
            fullname = fullpath.Split("/").Last();
            extension = fullname.Split(".").Last().ToLower();
            name = fullname.Substring(0, fullname.Length - extension.Length - 1);            
            //official = !Regex.IsMatch(fullpath, "(hacks|homebrew|demo|_alternatives|unlicensed)+", RegexOptions.IgnoreCase);
           
        }

        public void SetMra (MraInfo mraInfo)
        {
            this.mraInfo = mraInfo;
            this.date = mraInfo.date;            
            core = mraInfo.setname;
            region = mraInfo.region;
            checksum_md5 = mraInfo.md5;
            this.name = mraInfo.name;

        }
    }

    public class MraInfo
    {
        public string name { get; set; } = "";
        public string region { get; set; } = "";
        public bool homebrew { get; set; } = false;
        public bool bootleg { get; set; } = false;
        public string version { get; set; } = "";
        public string alternative { get; set; } = "";
        public string platform { get; set; } = "";
        public string series { get; set; } = "";
        public int year { get; set; } = 0;
        public string manufacturer { get; set; } = "";
        public string category { get; set; } = "";
        /// <summary>
        /// This indicates the name of the romset used as given by MAME. It is used to overwrite the core ID specified in the *.rbf file so that individual settings can be saved on a per romset basis.
        /// </summary>
        public string setname { get; set; } = "";
        public string parent { get; set; } = "";

        public string mameversion { get; set; } = "";
        /// <summary>
        /// his indicates the filename (sans path and extension) of the core that should be used to run the game
        /// </summary>
        public string rbf { get; set; } = "";
        public string about { get; set; } = "";
        public string resolution { get; set; } = "";
        public string rotation { get; set; } = "";
        public string flip { get; set; } = "";
        public string players { get; set; } = "";
        public string joystick { get; set; } = "";

        public DateTime date { get; set; } = DateTime.MinValue;      

        public List<string> romshierarchy { get; set; } = new List<string>();
        
        public string md5 { get; set; } = "";



    }
    
}
