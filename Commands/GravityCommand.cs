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

namespace Alpalis.AdminManager.Commands
{
    public class GravityCommand
    {
        #region Command Parameters
        [Command("gravity")]
        [CommandSyntax("<multipler> [player]")]
        [CommandDescription("Command to set gravity.")]
        [RegisterCommandPermission("other", Description = "Allows to set gravity of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class GravityUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public GravityUnturned(
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                         m_StringLocalizer["gravity_command:error_adminmode"]));
                if (Context.Parameters.Count == 1 || Context.Parameters.Count == 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                        m_StringLocalizer["gravity_command:error_multipler"]));
                if (Context.Parameters.Count == 1)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                        m_StringLocalizer["gravity_command:yourself", new { Multipler = multipler }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                        m_StringLocalizer["gravity_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                    m_StringLocalizer["gravity_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Multipler = multipler
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                    m_StringLocalizer["gravity_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        Multipler = multipler
                    }]));
            }
        }

        #region Command Parameters
        [Command("gravity")]
        [CommandSyntax("<multipler> <player>")]
        [CommandDescription("Command to set gravity.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class GravityConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public GravityConsole(
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IStringLocalizer stringLocalizer,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_StringLocalizer = stringLocalizer;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Parameters.Count == 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                        m_StringLocalizer["gravity_command:error_multipler"]));
                if (!Context.Parameters.TryGet(1, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                        m_StringLocalizer["gravity_command:error_player"]));
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                await UniTask.SwitchToMainThread();
                user.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["gravity_command:prefix"] : "",
                    m_StringLocalizer["gravity_command:somebody:console", new { Multipler = multipler }]));
                PrintAsync(m_StringLocalizer["gravity_command:somebody:executor", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    ID = identity,
                    Multipler = multipler
                }]);
            }
        }
    }
}
