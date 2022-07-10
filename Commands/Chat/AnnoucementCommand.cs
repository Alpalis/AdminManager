using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.UnityEngine.Extensions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Drawing;

namespace Alpalis.AdminManager.Commands.Chat
{
    public class AnnoucementCommand
    {
        #region Command Parameters
        [Command("annoucement")]
        [CommandDescription("Command to post annoucements.")]
        [CommandSyntax("<message>")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class AnnoucementConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public AnnoucementConsole(
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
                m_StringLocalizer = stringLocalizer;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count == 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
                if (message == "") throw new UserFriendlyException(m_StringLocalizer["annoucement_command:error_null_message"]);
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["annoucement_command:prefix"] : "",
                         message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(m_StringLocalizer["annoucement_command:succeed:executor"]);
            }
        }

        #region Command Parameters
        [Command("annoucement")]
        [CommandDescription("Command to post annoucements.")]
        [CommandSyntax("<message>")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class AnnoucementUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public AnnoucementUnturned(
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
                m_StringLocalizer = stringLocalizer;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count == 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["annoucement_command:prefix"] : "",
                         m_StringLocalizer["annoucement_command:error_adminmode"]));
                string message = string.Join(" ", Context.Parameters).Replace("</Color>", "").Replace("<color=", "");
                if (message == "") throw new UserFriendlyException(m_StringLocalizer["annoucement_command:error_null_message"]);
                await UniTask.SwitchToMainThread();
                ChatManager.serverSendMessage(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["annoucement_command:prefix"] : "",
                         message), Color.White.ToUnityColor(), null, null, EChatMode.GLOBAL, null, true);
                PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["annoucement_command:prefix"] : "",
                         m_StringLocalizer["annoucement_command:succeed:executor"]));
            }
        }
    }
}
