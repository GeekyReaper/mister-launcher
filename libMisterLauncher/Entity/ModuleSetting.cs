using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class ModuleSetting
    {
        public string _id { get; set; } = "";
        public string moduleName { get; set; } = "";
        public string valueType { get; set; } = "";
        public string name { get; set; } = "";
        public string value { get; set; } = "";
        public DateTime update { get; set; } = DateTime.Now;

        public string description { get; set; } = "";

        public void SetId ()
        {
            _id = string.Format("{0}-{1}", Service.Tools.RemoveSpecialCaracteres(moduleName), Service.Tools.RemoveSpecialCaracteres(name));
        }
    }
}
