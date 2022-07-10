using Alpalis.UtilityServices.Models;
using System;

namespace Alpalis.AdminManager.Models
{
    [Serializable]
    public class Config : MainConfig
    {
        public Config()
        {
            MessagePrefix = true;
        }

        public bool MessagePrefix { get; set; }

        public ushort AdminUIID { get; set; }

        public short AdminUIKey { get; set; }

        public ushort GodUIID { get; set; }

        public short GodUIKey { get; set; }

        public ushort VanishUIID { get; set; }

        public short VanishUIKey { get; set; }

        public ushort FlyUIID { get; set; }

        public short FlyUIKey { get; set; }
    }
}
