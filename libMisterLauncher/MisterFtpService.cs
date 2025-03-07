using FluentFTP;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

[assembly: InternalsVisibleTo("MisterLauncher.Test")]
namespace libMisterLauncher.Service
{


    public class FtpServiceSettings : IMisterSettings
    {
        const string _moduleName = "MisterFtp";
        public string ModuleName { get { return _moduleName; } }
        public string host { get; set; } = "";
        public string user { get; set; } = "root";
        public string password { get; set; } = "1";



        public bool isValid()
        {
            return !string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password);
        }

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "host",
                    value = host,
                    valueType = "text",
                    description = "ip or dns name",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "user",
                    value = user,
                    valueType = "text",
                    description = "default MiSTer value 'root'",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "password",
                    value = password,
                    valueType = "password",
                     description = "default MiSTer value '1'",
                    update = DateTime.Now
                },

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
                    case "host":
                        host = moduleSetting.value;
                        break;
                    case "user":
                        user = moduleSetting.value;
                        break;
                    case "password":
                        password = moduleSetting.value;
                        break;
                }
                

            }
        }
    }


    internal class MisterFtpService : BaseModule<FtpServiceSettings>
    {

        public MisterFtpService() : base()
        {

        }
        public MisterFtpService(FtpServiceSettings? settings) : base(settings)
        {         

        }

        public override bool CheckConnection()
        {

            using (var conn = new FtpClient(_settings.host, _settings.user, _settings.password))
            {
                conn.Config.ConnectTimeout = 2000;
                try
                {
                    conn.Connect();
                }
                catch (Exception ex)
                {
                    return false;
                }
               
            }
            return true;
        }
        
        public FtpClient CreateConnection ()
        {
            var conn = new FtpClient(_settings.host, _settings.user, _settings.password);
            conn.Connect();
            return conn;
        }

        public List<string> GetAvailableRomPath(FtpClient? conn = null)
        {
            var result = new List<string>();
            var initialmediapath = "/media/";
            var rompath = "games";

            bool localConnection = conn == null;

            if (localConnection)
            {
                conn = new FtpClient(_settings.host, _settings.user, _settings.password);
                conn.Connect();
            }
           
                
            foreach (var sourcepath in conn.GetListing(initialmediapath))
            {
                if (sourcepath.Type == FtpObjectType.Directory)
                {
                    if (Regex.IsMatch(sourcepath.Name, "^fat") || Regex.IsMatch(sourcepath.Name, @"^usb\d+"))
                    { // Check if sourcepath contains rompath
                        foreach (var subitem in conn.GetListing(initialmediapath + sourcepath.Name))
                        {
                            if (subitem.Type == FtpObjectType.Directory && subitem.Name == rompath)
                            {
                                result.Add(initialmediapath + sourcepath.Name + "/" + rompath + "/");
                            }
                        }
                    }
                }   
        }
                
            if (localConnection)
            {
                conn.Disconnect();
                conn.Dispose();
            }
            
            return result;
        }

        public List<Rom> GetConsoleRoms (SystemDb system, List<string>? availableRomPath = null, FtpClient? conn = null)
        {
            bool localConnection = conn == null;
            

            if (localConnection)
            {
                conn = new FtpClient(_settings.host, _settings.user, _settings.password);
                conn.Connect();
            }

            if (availableRomPath == null)
                availableRomPath = GetAvailableRomPath(conn);
            var result = new List<Rom>();

            if (availableRomPath.Count == 0)
                return result;

            
            foreach (var rompath in availableRomPath)
            {
                result.AddRange(
                    conn.GetListing(rompath + system.gamepath, FtpListOption.Recursive)
                    .Where(i => i.Type == FtpObjectType.File && ValidRomExtension(i.Name, system.GetAllowExtensions()) && CheckPaterns(system.GetExcludeRomPaterns(), i.Name))
                    .Select(i => MapToConsoleRom(i, system)));
            }
            
            if (localConnection)
            {
                conn.Disconnect();
                conn.Dispose();
            }

            return result;
        }

        public bool DeleteFile (string filepath)
        {
            var ftp = CreateConnection();
            try
            {
                ftp.DeleteFile(filepath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
            
        }

        internal bool CheckPaterns (List<string> paterns, string input)
        {
            if (paterns.Count == 0)
                return true;
            foreach (var p in paterns)
            {
                if (!string.IsNullOrEmpty(p) && Regex.IsMatch(input, p, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;

        }

        public List<Rom> GetArcadeRoms(FtpClient? conn = null)
        {
            bool localConnection = conn == null;


            if (localConnection)
            {
                conn = new FtpClient(_settings.host, _settings.user, _settings.password);
                conn.Connect();
            }

            var result = new List<Rom>();           
          

              result.AddRange(
                    conn.GetListing("/media/fat/_Arcade/", FtpListOption.Recursive)
                    .Where(i => i.Type == FtpObjectType.File && i.Name.EndsWith(".mra"))
            .Select(i =>
            {
                        var mrainfo = GetArcadeMraInfo(i.FullName, conn);
                        var rom = new Rom()
                        {
                            size = i.Size,
                            lastscandate = DateTime.Now,
                            systemCategory = "Arcade"
                        };
                        rom.SetFileName(i.FullName);
                        rom.SetMra(mrainfo);
                        bool isunoffical = mrainfo.bootleg || mrainfo.homebrew || !CheckPaterns(new List<string> { "(hacks|homebrew|demo|_alternatives|unlicensed)" }, i.FullName);
                        rom.official = !isunoffical;
                        rom.SetId();
                        return rom;
                    })
                    );

          

            if (localConnection)
            {
                conn.Disconnect();
                conn.Dispose();
            }

            return result;
        }

        public MraInfo GetArcadeMraInfo (string mrafullpath, FtpClient? conn = null)
        {
            bool localConnection = conn == null;
            var result = new MraInfo();

            if (localConnection)
            {
                conn = new FtpClient(_settings.host, _settings.user, _settings.password);
                conn.Connect();
            }


            if (conn.DownloadBytes(out byte[] bytes, mrafullpath))
            {
                string mracontents = Encoding.UTF8.GetString(bytes);
               result = ExtractMraInfo(mracontents);
            }

            if (localConnection)
            {
                conn.Disconnect();
                conn.Dispose();
            }

            return result;
        }

        public byte[] GetFile(string path, FtpClient? conn = null)
        {
            bool localConnection = conn == null;
            byte[] result = new byte[] { };

            if (localConnection)
            {
                conn = new FtpClient(_settings.host, _settings.user, _settings.password);
                conn.Connect();
            }


            if (conn.DownloadBytes(out byte[] bytes, path))
            {
                result = bytes;
            }

            if (localConnection)
            {
                conn.Disconnect();
                conn.Dispose();
            }

            return result;
        }

        internal bool ValidRomExtension (string romname, List<string> allowExtensions)
        {
            foreach (var e in allowExtensions)
            {
                if (romname.EndsWith("." + e))
                    return true;
            }
            return false;
        }

        internal Rom MapToConsoleRom (FtpListItem file, SystemDb system)
        {
            var result = new Rom
            {              
                size = file.Size,
                date = file.Modified,
                lastscandate = DateTime.Now,
                systemCategory = system.category,
                systemid = system._id,
                official = CheckPaterns(system.GetUnofficialPathRomPaterns(), file.FullName)
                
            };
            result.SetFileName(file.FullName);
            result.SetId();
            
            return result;

        }       

        internal MraInfo ExtractMraInfo (string xmlcontent)
        {
            // Fix some issue in MRA files
            xmlcontent = xmlcontent.Replace("</ROM>", "</rom>");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlcontent);

            var result = new MraInfo();

            var mainnode = xml.SelectSingleNode("misterromdescription");

            if (mainnode == null)
                return new MraInfo();

            result.name = extractXmlValue(mainnode, "name", "", s => s);
            result.region = extractXmlValue(mainnode, "region", "", s => s);
            result.homebrew = extractXmlValue(mainnode, "homebrew", false, s => s.ToLower() != "no");
            result.bootleg = extractXmlValue(mainnode, "bootleg", false, s => s.ToLower() != "no");
            result.version = extractXmlValue(mainnode, "version", "", s => s);
            result.alternative = extractXmlValue(mainnode, "alternative", "", s => s);
            result.platform = extractXmlValue(mainnode, "platform", "", s => s);
            result.series = extractXmlValue(mainnode, "series", "", s => s);
            result.year = extractXmlValue(mainnode, "year", 0, s => {
                var v = Regex.Replace(s, @"[^\d]", "");
                return int.Parse(v);
                });
            result.manufacturer = extractXmlValue(mainnode, "manufacturer", "", s => s);
            result.category = extractXmlValue(mainnode, "category", "", s => s);
            result.setname = extractXmlValue(mainnode, "setname", "", s => s);
            result.parent = extractXmlValue(mainnode, "parent", "", s => s);
            result.mameversion = extractXmlValue(mainnode, "mameversion", "", s => s);
            result.rbf = extractXmlValue(mainnode, "rbf", "", s => s);
            result.resolution = extractXmlValue(mainnode, "resolution", "", s => s);
            result.rotation = extractXmlValue(mainnode, "rotation", "", s => s);
            result.flip = extractXmlValue(mainnode, "flip", "", s => s);
            result.about = extractXmlValue(mainnode, "about", "", s => s);
            result.players = extractXmlValue(mainnode, "players", "", s => s);
            result.joystick = extractXmlValue(mainnode, "joystick", "", s => s);            

            var t = extractXmlValue(mainnode, "mratimestamp", "", s => s);
            if (!string.IsNullOrEmpty(t))
            {
                try
                {
                    var year = int.Parse(t.Substring(0, 4));
                    var month = int.Parse(t.Substring(4, 2));
                    var day = int.Parse(t.Substring(6, 2));
                    var hour = t.Length >= 10 ? int.Parse(t.Substring(8, 2)) : 0;
                    var min = t.Length >= 12 ? int.Parse(t.Substring(10, 2)) : 0;
                    var sec = t.Length >= 14 ? int.Parse(t.Substring(12, 2)) : 0;
                    result.date = new DateTime(year, month, day, hour, min, sec);
                }
                catch (Exception ex)
                {
                    result.date = DateTime.MinValue;
                }
            }

           

            var romNode = mainnode.SelectSingleNode("rom[@index=0]");
            if (romNode != null)
            {
                if (romNode.Attributes["md5"] != null && romNode.Attributes["md5"].Value.ToLower()!="none")
                {
                    result.md5 = romNode.Attributes["md5"].Value;
                }
                if (romNode.Attributes["zip"] != null)
                {
                    result.romshierarchy.AddRange(romNode.Attributes["zip"].Value.Split("|"));
                   
                }
            }
           

            return result;




        }       

        internal T extractXmlValue<T>(XmlNode xml, string path, T defaultvalue, Func<string,T> castfromstring)
        {
            var node = xml.SelectSingleNode(path);
            if (node!=null)
            {
                try
                {
                    return castfromstring(node.InnerText);
                }
                catch (Exception ex)
                {
                    return default;
                }
            }
            else
            {
                return defaultvalue;
            }
        }
        internal T extractAttributesValue<T>(XmlNode xml, string path, T defaultvalue, Func<string, T> castfromstring)
        {
            var node = xml.SelectSingleNode(path);
            if (node != null)
            {
                return castfromstring(node.InnerText);
            }
            else
            {
                return defaultvalue;
            }
        }
    }
}
