using Alpalis.UtilityServices.Models;
using System;

namespace Alpalis.AdminManager.Models
{
    [Serializable]
    public class Config : MainConfig
    {
        public Config()
        {
            IdentityManagerImplementation = false;
        }
        public ushort AdminUIID { get; set; }

        public short AdminUIKey { get; set; }

        public ushort GodUIID { get; set; }

        public short GodUIKey { get; set; }

        public ushort VanishUIID { get; set; }

        public short VanishUIKey { get; set; }

        public bool IdentityManagerImplementation { get; set; }
    }
}
