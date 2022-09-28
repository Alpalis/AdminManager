using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.AdminManager.Services;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Net;

namespace Alpalis.AdminManager.Commands
{
    #region Commad Parameters
    [Command("exit")]
    [CommandDescription("Command to fast exit the server.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class ExitCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IAdminSystem m_AdminSystem;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public ExitCommand(
            IStringLocalizer StringLocalizer,
            IConfigurationManager configurationManager,
            IAdminSystem adminSystem,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = StringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_AdminSystem = adminSystem;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 0)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["exit_command:prefix"] : "",
                     m_StringLocalizer["exit_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            Provider.kick(user.SteamId, m_StringLocalizer["exit_command:message"]);
        }
    }
}
