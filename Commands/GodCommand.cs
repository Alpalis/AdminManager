using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Command Parameters
    [Command("god")]
    [CommandDescription("Command to turn on and off the god mode.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class GodCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IGodSystem m_GodSystem;
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        #endregion Member Variables

        #region Class Constructor
        public GodCommand(
            IGodSystem godSystem,
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_GodSystem = godSystem;
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(m_StringLocalizer["god_mode:error_adminmode"]);
            if (Context.Parameters.Length != 0) throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (m_GodSystem.IsInGodMode(user.SteamId))
            {
                m_GodSystem.DisableGodMode(user.Player.SteamPlayer);
                PrintAsync(m_StringLocalizer["god_mode:disabled"]);
                return;
            }
            m_GodSystem.EnableGodMode(user.Player.SteamPlayer);
            PrintAsync(m_StringLocalizer["god_mode:enabled"]);
        }
    }
}
