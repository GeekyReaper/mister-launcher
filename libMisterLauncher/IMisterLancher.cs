using libMisterLauncher.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libMisterLauncher.Service
{

    public enum RomAction { UNLINK, SETPRIMARY, LINK };

    public struct ModuleDefinition
    {
        public Type ModuleType;
        public Type SettingsType;
    }

    public static class Tools
    {
        public static string RemoveSpecialCaracteres (string value)
        {
            return Regex.Replace(value, "[^A-Za-z0-9]+", "").ToLower();            
        }
        public static string ReplaceSpecialCaracteres(string value, string caractere = "-")
        {
            var t = Regex.Replace(value, "[^A-Za-z0-9]+", caractere);
            while (t.Length > 0 && t.EndsWith("-"))
            {
                t = t.Substring(0, t.Length - 1);
            }
            return t.ToLower();
        }

        public static DateTime ParseDate (string value)
        {
            var result = DateTime.MinValue;
            if (Regex.IsMatch(value, @"^\d{2}$"))
            {
                result = new DateTime(1900 + int.Parse(value), 1, 1);
            }
            else if (Regex.IsMatch(value, @"^\d{4}$"))
            {
                result = new DateTime(int.Parse(value), 1, 1);
            }
            else if (Regex.IsMatch(value, @"^\d{4}-\d{2}$"))
            {
                bool v1 = DateTime.TryParseExact(value, "yyyy-MM", new CultureInfo("fr-FR"), DateTimeStyles.None, out result);
            }
            else
            {
                bool v = DateTime.TryParseExact(value, "yyyy-MM-dd", new CultureInfo("fr-FR"), DateTimeStyles.None, out result);
            }
            if (result > DateTime.MinValue)
                result.AddHours(2);
            return result;
        }

    }
    
    public static class Cast
    {
        public static RomDb ConvertRom(Rom rom)
        {
            var result = new RomDb()
            {
                romid = rom._id,
                date = rom.date,
                size = (int)rom.size,
                fullpath = rom.fullpath,
                name = rom.name,
                region = string.IsNullOrEmpty(rom.region) && (rom.mraInfo != null) ? rom.mraInfo.region : rom.region,
                language = rom.language,
                supportType = rom.supporttype,
                core = rom.core
            };

            return result;
        }
    }
    
    public interface IMisterSettings
    {
        public string ModuleName { get; }
        public bool isValid();
        public List<ModuleSetting> GetModuleSettings();
        public static List<ModuleSetting> GetDefaultModuleSettings() { return new List<ModuleSetting>(); }       
       public void LoadModuleSettings (List<ModuleSetting> moduleSettings);
    }

    public interface IMisterModule
    {
        public bool CheckConnection();
        public MisterModuleHealthCheck CheckHealth();

        public void LoadSettings(IMisterSettings settings);
    }

    public class BaseModule<T> : IMisterModule where T : IMisterSettings, new()
    {
        internal T _settings = new T();
        internal string _moduleName = "";
        internal MisterModuleHealthCheck _health;

        public MisterModuleHealthCheck Health { get { return _health; } }

        internal bool isOk { get { return _health.MisterState == MisterStateEnum.OK; } }

        public BaseModule()
        {
            _health = new MisterModuleHealthCheck("basemodule");
        }

        public BaseModule(T? settings)
        {
            if (settings != null)
                _settings = settings;
            _moduleName = settings.ModuleName;
            _health = new MisterModuleHealthCheck(_moduleName);
        }

        public virtual void LoadSettings(IMisterSettings settings)
        {
            if (settings != null)
                _settings = (T)settings;
            _moduleName = _settings.ModuleName;
            _health = new MisterModuleHealthCheck(_moduleName);
        }

        public List<ModuleSetting> GetModuleSettings ()
        {
            return _settings.GetModuleSettings();
        }

        public virtual MisterModuleHealthCheck CheckHealth()
        {
            if (!_settings.isValid())
            {
                _health.MisterState = MisterStateEnum.ERROR;
                _health.Message = "Invalid settings";
                return _health;
            }
            if (!CheckConnection())
            {
                _health.MisterState = MisterStateEnum.ERROR;
                _health.Message = "Connection error";
                return _health;
            }
            _health.MisterState = MisterStateEnum.OK;

            return _health;
        }

        public virtual bool CheckConnection()
        {
            return true;
        }
     
    }
}
