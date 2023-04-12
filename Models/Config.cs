using Alpalis.UtilityServices.Models;
using System;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Models
{
    [Serializable]
    public class Config : MainConfig
    {
        public Config()
        {
            DisableAdminMode = false;
        }

        public bool DisableAdminMode { get; set; }

        public override List<KeyValuePair<string, string>> GetPropertiesInString()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new ("DisableAdminMode", $"{DisableAdminMode}")
            };
        }
    }
}
