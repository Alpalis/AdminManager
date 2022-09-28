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
    [Command("join")]
    [CommandDescription("Command to join another server.")]
    [CommandSyntax("<ip/domain> <port> [password]")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class JoinCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IAdminSystem m_AdminSystem;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public JoinCommand(
            IStringLocalizer StringLocalizer,
            IAdminSystem adminSystem,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = StringLocalizer;
            m_AdminSystem = adminSystem;
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
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out string? ipString) || ipString == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_ip"]));
            IPAddress[]? addresses = null;
            try
            {
                addresses = Dns.GetHostAddresses(ipString);
            }
            catch (Exception) { }
            if (addresses == null || addresses.Length == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_ip"]));
            if (!Context.Parameters.TryGet(1, out ushort port))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["join_command:prefix"] : "",
                     m_StringLocalizer["join_command:error_port"]));
            string? password = Context.Parameters.Count == 3 ? Context.Parameters[2] : default;
            await UniTask.SwitchToMainThread();
            var ipBytes = addresses[0].GetAddressBytes();
            uint ipUint = (uint)ipBytes[0] << 24;
            ipUint += (uint)ipBytes[1] << 16;
            ipUint += (uint)ipBytes[2] << 8;
            ipUint += (uint)ipBytes[3];
            user.Player.Player.sendRelayToServer(ipUint, port, password, false);
        }
    }
}
