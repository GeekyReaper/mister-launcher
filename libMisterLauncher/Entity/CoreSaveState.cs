using libMisterLauncher.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class CoreSaveState
    {
        public string _id { get; set; } = "";
        public int slot { get; set; } = 1;
        public DateTime Modified { get; set; } = DateTime.MinValue;
        public string mediaId { get; set; } = "";

        public string core { get; set; }
        public string filename { get; set; }
        public string videogameid { get; set; }
        public string romid { get; set; }
        public string systemid { get; set; }

        public bool isEmpty { get { return string.IsNullOrEmpty(filename); } }

        public void setId ()
        {
            _id = Tools.ReplaceSpecialCaracteres(videogameid, "-")
                + "/" + Tools.ReplaceSpecialCaracteres(romid, "-")
                + string.Format(" [{0}]", slot);
        }       

    }
}
