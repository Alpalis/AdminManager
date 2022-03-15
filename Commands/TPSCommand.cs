using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Command Parameters
    [Command("tps")]
    [CommandDescription("Command that shows TPS.")]
    #endregion Command Parameters
    public class TPSCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public TPSCommand(
            IConfigurationManager configurationManager,
            IStringLocalizer stringLocalizer,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_ConfigurationManager = configurationManager;
            m_StringLocalizer = stringLocalizer;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            int tps = Provider.debugTPS;
            PrintAsync(string.Format("{0}{1}", 
                config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["tps_command:prefix"] : "",
                m_StringLocalizer["tps_command:succeed", new { TPS = tps }]));
        }
    }
}
