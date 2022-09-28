using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
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

namespace Alpalis.AdminManager.Commands
{
    public class TimeCommand
    {
        #region Commad Parameters
        [Command("time")]
        [CommandSyntax("<get/set>")]
        [CommandDescription("Command to change or get overworld time.")]
        #endregion Command Parameters
        public class TimeRoot : UnturnedCommand
        {
            #region Class Constructor
            public TimeRoot(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }
        }

        #region Commad Parameters
        [Command("get")]
        [CommandDescription("Command to get overworld time.")]
        [CommandParent(typeof(TimeRoot))]
        #endregion Command Parameters
        public class TimeGet : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TimeGet(
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IAdminSystem adminSystem,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
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
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["time_command:prefix"] : "",
                         m_StringLocalizer["time_command:error_adminmode"]));
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                    m_StringLocalizer["time_command:get", new
                    {
                        Time = LightingManager.time
                    }]));
            }
        }

        #region Commad Parameters
        [Command("set")]
        [CommandSyntax("<time>")]
        [CommandDescription("Command to set overworld time.")]
        [CommandParent(typeof(TimeRoot))]
        #endregion Command Parameters
        public class TimeSet : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TimeSet(
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
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["time_command:prefix"] : "",
                         m_StringLocalizer["time_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                if (!Context.Parameters.TryGet(0, out uint time))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                        m_StringLocalizer["time_command:set:error_time"]));
                if (time >= 3600)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                        m_StringLocalizer["time_command:set:error_maxtime"]));
                LightingManager.time = time;
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["time_command:prefix"] : "",
                    m_StringLocalizer["time_command:set:succeed", new
                    {
                        Time = time
                    }]));
            }
        }
    }
}
