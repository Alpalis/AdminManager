using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Commad Parameters
    [Command("adminmode")]
    [CommandDescription("Command to turn on and off the administrator mode.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class AdminModeCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        #endregion Member Variables

        #region Class Constructor
        public AdminModeCommand(
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0) throw new CommandWrongUsageException(Context);
            UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
            await UniTask.SwitchToMainThread();
            if (m_AdminSystem.ToggleAdminMode(uPlayer.Player.SteamPlayer))
            {
                PrintAsync(m_StringLocalizer["admin_mode:enabled"]);
                return;
            }
            PrintAsync(m_StringLocalizer["admin_mode:disabled"]);
        }
    }
}
