using libMisterLauncher.Entity;
using System;
using System.Collections.Generic;

namespace libMisterLauncher.Service
{
    public class GeneralSettingsServiceSettings : IMisterSettings
    {
        const string _moduleName = "GeneralSettings";
        public string ModuleName { get { return _moduleName; } }

        public TimeSpan RefreshConnection { get; set; } = new TimeSpan(0);

        public string publicBaseUrl { get; set; } = "";
        public bool allowAnonymousMedia { get; set; } = false;

        public bool isValid()
        {
            return true;
        }

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "publicBaseUrl",
                    value = publicBaseUrl,
                    valueType = "url",
                    description = "Base URL used to build absolute media links (screenshots, videos, manuals) exposed to external tools such as Zaparoo (e.g. https://192.168.1.50:6002)",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "allowAnonymousMedia",
                    value = allowAnonymousMedia.ToString(),
                    valueType = "checkbox",
                    description = "Allow GET /api/media/{id} without a JWT token — needed for external tools (Zaparoo) that cannot authenticate interactively",
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
                    case "publicBaseUrl":
                        publicBaseUrl = moduleSetting.value;
                        break;
                    case "allowAnonymousMedia":
                        bool.TryParse(moduleSetting.value, out bool allow);
                        allowAnonymousMedia = allow;
                        break;
                }
            }
        }
    }

    public class GeneralSettingsService : BaseModule<GeneralSettingsServiceSettings>
    {
        public GeneralSettingsService() : base() { }
        public GeneralSettingsService(GeneralSettingsServiceSettings? settings) : base(settings) { }

        public string PublicBaseUrl { get { return _settings.publicBaseUrl; } }
        public bool AllowAnonymousMedia { get { return _settings.allowAnonymousMedia; } }
    }
}
