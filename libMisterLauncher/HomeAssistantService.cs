using libMisterLauncher.Entity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace libMisterLauncher.Service
{
    public class HaSwitchState
    {
        public string State { get; set; } = "";
        public DateTime? LastChanged { get; set; }
    }

    public class HomeAssistantServiceSettings : IMisterSettings
    {
        const string _moduleName = "HomeAssistant";
        public string ModuleName { get { return _moduleName; } }

         public TimeSpan RefreshConnection { get; set; } = new TimeSpan(0);

        public string url { get; set; } = "";
        public string token { get; set; } = "";
        public string switchEntity { get; set; } = "";

        public bool isValid()
        {
            return !string.IsNullOrEmpty(url)
                && !string.IsNullOrEmpty(token)
                && !string.IsNullOrEmpty(switchEntity);
        }

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "url",
                    value = url,
                    valueType = "url",
                    description = "Home Assistant base URL (e.g. http://homeassistant.local:8123)",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "token",
                    value = token,
                    valueType = "password",
                    description = "Long-lived access token generated in your Home Assistant profile",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "switchEntity",
                    value = switchEntity,
                    valueType = "text",
                    description = "Entity ID of the switch controlling MiSTer power (e.g. switch.mister_power)",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "refreshConnection",
                    value = "300",
                    description = "Time in second, between two check connection",
                    valueType = "number",
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
                    case "url":
                        url = moduleSetting.value;
                        break;
                    case "token":
                        token = moduleSetting.value;
                        break;
                    case "switchEntity":
                        switchEntity = moduleSetting.value;
                        break;
                    case "refreshConnection":
                        if (int.TryParse(moduleSetting.value, out int refreshInterval))
                            RefreshConnection = TimeSpan.FromSeconds(refreshInterval);
                        break;
                }
            }
        }
    }

    public class HomeAssistantService : BaseModule<HomeAssistantServiceSettings>
    {
        public HomeAssistantService() : base() { }
        public HomeAssistantService(HomeAssistantServiceSettings? settings) : base(settings) { }

        public override MisterModuleHealthCheck CheckHealth()
        {
            if (string.IsNullOrEmpty(_settings.url)
                && string.IsNullOrEmpty(_settings.token)
                && string.IsNullOrEmpty(_settings.switchEntity))
            {
                _health.MisterState = MisterStateEnum.DISABLE;
                _health.Message = "Not configured";
                return _health;
            }
            return base.CheckHealth();
        }

        public override bool CheckConnection()
        {
            try
            {
                using var client = BuildHttpClient();

                // Check API is reachable
                var apiResponse = client.GetAsync($"{_settings.url.TrimEnd('/')}/api/").Result;
                if (!apiResponse.IsSuccessStatusCode)
                    return false;

                // Check entity exists and is a switch
                var stateResponse = client.GetAsync($"{_settings.url.TrimEnd('/')}/api/states/{_settings.switchEntity}").Result;
                if (!stateResponse.IsSuccessStatusCode)
                {
                    _health.Message = $"Entity '{_settings.switchEntity}' not found";
                    return false;
                }

                var json = stateResponse.Content.ReadAsStringAsync().Result;
                var node = JsonNode.Parse(json);
                var entityId = node?["entity_id"]?.GetValue<string>() ?? "";
                if (!entityId.StartsWith("switch.", StringComparison.OrdinalIgnoreCase))
                {
                    _health.Message = $"Entity '{_settings.switchEntity}' is not of type switch";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _health.Message = ex.Message;
                return false;
            }
        }

        private HttpClient BuildHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.token);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public async Task<HaSwitchState?> GetSwitchState()
        {
            try
            {
                using var client = BuildHttpClient();
                var response = await client.GetAsync($"{_settings.url.TrimEnd('/')}/api/states/{_settings.switchEntity}");
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                var node = JsonNode.Parse(json);
                var state = node?["state"]?.GetValue<string>();
                var lastChanged = node?["last_changed"]?.GetValue<string>();
                if (state == null) return null;
                return new HaSwitchState
                {
                    State = state,
                    LastChanged = lastChanged != null ? DateTime.Parse(lastChanged, null, System.Globalization.DateTimeStyles.RoundtripKind) : null
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> TurnOn()
        {
            return await CallService("turn_on");
        }

        public async Task<bool> TurnOff()
        {
            return await CallService("turn_off");
        }

        public async Task<bool> Toggle()
        {
            return await CallService("toggle");
        }

        private async Task<bool> CallService(string action)
        {
            using var client = BuildHttpClient();
            var body = new StringContent(
                JsonSerializer.Serialize(new { entity_id = _settings.switchEntity }),
                System.Text.Encoding.UTF8,
                "application/json");
            var response = await client.PostAsync(
                $"{_settings.url.TrimEnd('/')}/api/services/switch/{action}", body);
            return response.IsSuccessStatusCode;
        }
    }
}
