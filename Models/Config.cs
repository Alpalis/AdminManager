using Alpalis.UtilityServices.Models;
using System;

namespace Alpalis.AdminManager.Models
{
    [Serializable]
    public class Config : MainConfig
    {
        #region Class Constructor
        public Config()
        {
            MessagePrefix = true;
            AdminUIID = 29000;
            AdminUIKey = 0;
            GodUIID = 29001;
            GodUIKey = 1;
            VanishUIID = 29002;
            VanishUIKey = 2;
            FlyUIID = 29003;
            FlyUIKey = 3;
        }
        #endregion Class Constructor

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
