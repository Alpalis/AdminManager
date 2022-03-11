using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Command Parameters
    [Command("vanish")]
    [CommandDescription("Command to turn vanish mode on and off.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class VanishCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IVanishSystem m_VanishSystem;
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        #endregion Member Variables

        #region Class Constructor
        public VanishCommand(
            IVanishSystem vanishSystem,
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_VanishSystem = vanishSystem;
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(m_StringLocalizer["vanish_mode:error_adminmode"]);
            if (Context.Parameters.Length != 0) throw new CommandWrongUsageException(Context);
            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            if (m_VanishSystem.IsInVanishMode(uPlayer.SteamId))
            {
                m_VanishSystem.DisableVanishMode(uPlayer.Player.SteamPlayer);
                PrintAsync(m_StringLocalizer["vanish_mode:disabled"]);
                return;
            }
            m_VanishSystem.EnableVanishMode(uPlayer.Player.SteamPlayer);
            PrintAsync(m_StringLocalizer["vanish_mode:enabled"]);
        }
    }
}
