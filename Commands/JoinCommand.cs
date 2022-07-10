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
using System.Net;

namespace Alpalis.AdminManager.Commands
{
    #region Commad Parameters
    [Command("join")]
    [CommandDescription("Command to join another server.")]
    [CommandSyntax("<ip> <port> [password]")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class JoinCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public JoinCommand(
            IStringLocalizer StringLocalizer,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = StringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2 && Context.Parameters.Count != 3)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!Context.Parameters.TryGet(0, out string? ipString) || ipString == null || !IPAddress.TryParse(ipString, out IPAddress ip))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_ip"]));
            if (!Context.Parameters.TryGet(1, out ushort port))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_port"]));
            string? password = Context.Parameters.Count == 3 ? Context.Parameters[2] : default;
            await UniTask.SwitchToMainThread();
            user.Player.Player.sendRelayToServer(BitConverter.ToUInt32(ip.GetAddressBytes(), 0), port, password);
        }
    }
}
