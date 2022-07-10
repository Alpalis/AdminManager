using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands.Life
{
    public class VirusCommand
    {
        #region Command Parameters
        [Command("virus")]
        [CommandSyntax("[player]")]
        [CommandDescription("Command to set virus for yourself and other players.")]
        [RegisterCommandPermission("other", Description = "Allows to set virus of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class VirusUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public VirusUnturned(
                IAdminSystem adminSystem,
                IIdentityManagerImplementation identityManagerImplementation,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                         m_StringLocalizer["virus_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.life.serverModifyVirus(100);
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                        m_StringLocalizer["virus_command:yourself"]));
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                        m_StringLocalizer["virus_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.life.serverModifyFood(100);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                    m_StringLocalizer["virus_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                    m_StringLocalizer["virus_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity
                    }]));
            }
        }

        #region Command Parameters
        [Command("virus")]
        [CommandSyntax("<player>")]
        [CommandDescription("Command to set virus of other players.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class VirusConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public VirusConsole(
                IIdentityManagerImplementation identityManagerImplementation,
                IStringLocalizer stringLocalizer,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_StringLocalizer = stringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["virus_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                user.Player.Player.life.serverModifyVirus(100);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["virus_command:prefix"] : "",
                    m_StringLocalizer["virus_command:somebody:console"]));
                PrintAsync(m_StringLocalizer["virus_command:somebody:executor", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity
                }]);
            }
        }
    }
}
