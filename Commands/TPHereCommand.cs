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
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Command Parameters
    [Command("tphere")]
    [CommandDescription("Command that teleports player to you.")]
    [CommandSyntax("<player>")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class TPHereCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public TPHereCommand(
            IAdminSystem adminSystem,
            IIdentityManagerImplementation identityManagerImplementation,
            IConfigurationManager configurationManager,
            IStringLocalizer stringLocalizer,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_IdentityManagerImplementation = identityManagerImplementation;
            m_ConfigurationManager = configurationManager;
            m_StringLocalizer = stringLocalizer;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["tphere_command:prefix"] : "",
                     m_StringLocalizer["tphere_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? teleportUser) || teleportUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["tphere_command:prefix"] : "",
                    m_StringLocalizer["tphere_command:error_player"]));
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
            SteamPlayer teleportSPlayer = teleportUser.Player.SteamPlayer;
            CSteamID teleportSteamID = teleportSPlayer.playerID.steamID;
            ushort? teleportIdentity = m_IdentityManagerImplementation.GetIdentity(teleportSteamID);
            teleportUser.Player.Player.TeleportToLocationAsync(user.Player.Player.transform.position);
            teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                config.MessagePrefix ? m_StringLocalizer["tphere_command:prefix"] : "",
                m_StringLocalizer["tphere_command:succeed:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity
                }]));
            PrintAsync(string.Format("{0}{1}",
                config.MessagePrefix ? m_StringLocalizer["tphere_command:prefix"] : "",
                m_StringLocalizer["tphere_command:succeed:executor", new
                {
                    PlayerName = teleportSPlayer.playerID.playerName,
                    CharacterName = teleportSPlayer.playerID.characterName,
                    NickName = teleportSPlayer.playerID.nickName,
                    SteamID = teleportSteamID,
                    ID = teleportIdentity
                }]));
        }
    }
}
