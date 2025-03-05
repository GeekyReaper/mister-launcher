using libMisterLauncher.Entity;
using libMisterLauncher.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher
{
    public class AuthServiceSettings : IMisterSettings
    {
        const string _moduleName = "MisterAuth";
        public string ModuleName { get { return _moduleName; } }
        public string adminpassword { get; set; } = "";

        public TimeSpan PendingExpireTime { get; set; } = new TimeSpan(0,10,0);
        public TimeSpan ApprouvalExpireTime { get; set; } = new TimeSpan(0, 10, 0);
        public TimeSpan PersistTime { get; set; } = new TimeSpan(168, 0, 0);

        public int TokenAdminDelay { get; set; } = 1080;
        public int TokenGuestDelay { get; set; } = 180;

        public int NumberOfDigit { get; set; } = 4;

        public int ConsumeMaxRetry { get; set; } = 5;
        //public int guesttokenexpire { get; set; } = 120;


        public bool isValid()
        {
            return !string.IsNullOrEmpty(adminpassword);
        }

      

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "adminpassword",
                    value = adminpassword,
                    valueType = "password",
                    description = "Complete a password to enable authentication. There is no complexity check.",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "pendingexpiretime",
                    value = ((int)PendingExpireTime.TotalMinutes).ToString(),
                    valueType = "number",
                    description = "Expiration time of an guest request expressed in minutes, before being approved or denied by the administrator.",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "approuvalexpiretime",
                    value = ((int)ApprouvalExpireTime.TotalMinutes).ToString(),
                    valueType = "number",
                    description = "Expiration time of an guest request expressed in minutes, after being approuved and waiting to be consumed",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "persisttime",
                    value = ((int)ApprouvalExpireTime.TotalHours).ToString(),
                    valueType = "number",
                    description = "Time in hours during which the guest request is stored once it has reached a final status.",
                    update = DateTime.Now
                },
                //new ModuleSetting()
                //{
                //    moduleName = _moduleName,
                //    name = "guesttokenexpire",
                //    value = guesttokenexpire.ToString(),
                //    valueType = "number",
                //    description = "in minutes",
                //    update = DateTime.Now
                //},
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "numberofdigit",
                    value = NumberOfDigit.ToString(),
                    valueType = "number",
                    description = "Size of Guest Code",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "consumemaxretry",
                    value = ConsumeMaxRetry.ToString(),
                    valueType = "number",
                    description = "Number of retry authorize to Consume guest acess.",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "tokenadmindelay",
                    value = TokenAdminDelay.ToString(),
                    valueType = "number",
                    description = "Number of minute for Admin token lifetime",
                    update = DateTime.Now
                },
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "tokenguestdelay",
                    value = TokenGuestDelay.ToString(),
                    valueType = "number",
                    description = "Number of minute for Guest token lifetime",
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
                int intparse = 0;
                switch (moduleSetting.name)
                {
                    case "adminpassword":
                        adminpassword = moduleSetting.value;
                        break;
                    case "pendingexpiretime":
                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            PendingExpireTime = new TimeSpan(0, intparse, 0);
                        }                        
                        break;
                    case "approuvalexpiretime":

                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            ApprouvalExpireTime = new TimeSpan(0, intparse, 0);
                        }                        
                        break;
                    case "persisttime":

                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            ApprouvalExpireTime = new TimeSpan(intparse, 0, 0);
                        }
                        break;
                    //case "guesttokenexpire":
                    //    if (int.TryParse(moduleSetting.value, out intparse))
                    //    {
                    //        guesttokenexpire = intparse;
                    //    }
                    //    break;
                    case "numberofdigit":
                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            NumberOfDigit = intparse;
                        }                       
                        break;
                    case "consumemaxretry":
                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            ConsumeMaxRetry = intparse;
                        }
                        break;
                    case "tokenadmindelay":
                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            TokenAdminDelay = intparse;
                        }
                        break;
                    case "tokenguestdelay":
                        if (int.TryParse(moduleSetting.value, out intparse))
                        {
                            TokenGuestDelay = intparse;
                        }
                        break;
                }
            }
        }
    }

    public class MisterAuthService : BaseModule<AuthServiceSettings>
    {
        
        internal List<GuestAccess> GuestAccessList { get; set; } = new List<GuestAccess>();

        public MisterAuthService() : base()
        {

        }
        public MisterAuthService(AuthServiceSettings? settings) : base(settings)
        {

        }


        public int GetTokenDelay(TokenType type)
        {
            switch (type)
            {
                case TokenType.ADMIN:
                    return _settings.TokenAdminDelay;
                case TokenType.GUEST:
                    return _settings.TokenGuestDelay;
            }
            return 0;
        }

        public override bool CheckConnection()
        {
            return GuestAccessList.Count < Math.Pow(10, _settings.NumberOfDigit);
        }

        public override MisterModuleHealthCheck CheckHealth()
        {
            if (!CheckConnection())
            {
                _health.MisterState = MisterStateEnum.ERROR;
                _health.Message = "Too much Guest Access request in queue";
                return _health;
            }

            if (!_settings.isValid())
            {
                _health.MisterState = MisterStateEnum.WARNING;
                _health.Message = "Authentication is not set";
                return _health;
            }

            _health.MisterState = MisterStateEnum.OK;

            return _health;
        }

        internal List<int> getUsedCodeIdx ()
        {            
            if (GuestAccessList.Count == 0)
                return new List<int>();
            return GuestAccessList.Select(c => c.CodeIdx).OrderBy(c => c).ToList();
        }

        internal int PurgeGuestAccess()
        {
            return GuestAccessList.RemoveAll(g => g.hasExpired());

        }

        public bool CheckAdminPassword (string password)
        {
            return _settings.adminpassword == password;
        }

        public GuestAccess? GenerateGuestAccess (string signature)
        {
            int maxvalue = (int) Math.Pow(10,_settings.NumberOfDigit);
            
            var result = new GuestAccess();
            PurgeGuestAccess();
            var codeIdxUnavailable = getUsedCodeIdx();
            if (codeIdxUnavailable.Count == maxvalue) // All code is already used
                return null;
            var maxidx = (maxvalue-1) - codeIdxUnavailable.Count;
            var newidx = Random.Shared.Next(maxidx);
            var idxisok = false;
            while (!idxisok)
            {
                int i = 0;
                while(!idxisok && i< codeIdxUnavailable.Count)
                {
                    if (newidx < codeIdxUnavailable[i])
                    {
                        idxisok = true;
                    }
                    else
                    {
                        newidx++;
                        i++;
                    }                    
                }
                if (newidx <= maxvalue-1)
                {
                    idxisok = true;
                }
                else
                {
                    newidx = (newidx % maxidx);
                }
            }

            result.Create(newidx, signature, _settings.PendingExpireTime, maxvalue-1);

            GuestAccessList.Add(result);


            return result;

        }

        public List<GuestAccess> GetCurrentGuestAccess()
        {
            return GuestAccessList.OrderByDescending(ga => ga.Created).ToList();
        }

        public GuestAccess? GetGuestAccess(string code)
        {
            return GuestAccessList.Where(g => g.Code == code).FirstOrDefault();
        }

        public GuestAccessState GuestAccessState(string code)
        {
            var ga = GuestAccessList.Where(g => g.Code == code).FirstOrDefault();
            return ga != null ? ga.State : Entity.GuestAccessState.NOTFOUND;            
        }

        public bool GuessAccessAdminApprouve(bool approuved, string code)
        {
            var ga =  GuestAccessList.Where(g => g.Code == code).FirstOrDefault();
            if (ga==null || !ga.isValid())
                return false;
            return  ga.Approuved(approuved, approuved ? _settings.ApprouvalExpireTime : _settings.PersistTime);            
        }
        public bool GuestAccessClientConsumed (string code, string key)
        {
            var ga = GuestAccessList.Where(g => g.Code == code).FirstOrDefault();
            if (ga == null || !ga.isValid())
                return false;
            return ga.Consumed(key, _settings.PersistTime, _settings.ConsumeMaxRetry);            
        }
         

        

    }
}
