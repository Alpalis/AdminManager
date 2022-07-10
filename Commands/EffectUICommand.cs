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
    public class UIEffectCommand
    {
        #region Command Parameters
        [Command("effectui")]
        [CommandAlias("effui")]
        [CommandDescription("Displays UI effect.")]
        [CommandSyntax("<id> <key> [player]")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to display UI effect for other peoples.")]
        #endregion Command Parameters
        public class UIEffectUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public UIEffectUnturned(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count < 2 || Context.Parameters.Count > 3)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                         m_StringLocalizer["effectui_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out ushort id))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                         m_StringLocalizer["effectui_command:error_id"]));
                if (!Context.Parameters.TryGet(1, out short key))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                         m_StringLocalizer["effectui_command:error_key"]));
                if (Context.Parameters.Count == 2)
                {
                    await UniTask.SwitchToMainThread();
                    EffectManager.sendUIEffect(id, key, user.Player.Player.channel.GetOwnerTransportConnection(), true);
                    PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                         m_StringLocalizer["effectui_command:succeed:yourself", new
                         {
                             EffectID = id,
                             EffectKey = key
                         }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                        m_StringLocalizer["effectui_command:error_player"]));
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(id, key, targetUser.Player.Player.channel.GetOwnerTransportConnection(), true);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                    m_StringLocalizer["effectui_command:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        EffectID = id,
                        EffectKey = key
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                    m_StringLocalizer["effectui_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        EffectID = id,
                        EffectKey = key
                    }]));
            }
        }

        #region Command Parameters
        [Command("effectui")]
        [CommandAlias("effui")]
        [CommandDescription("Displays UI effect.")]
        [CommandSyntax("<id> <key> <player>")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class UIEffectConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public UIEffectConsole(
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 3)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (!Context.Parameters.TryGet(0, out ushort id))
                    throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_id"]);
                if (!Context.Parameters.TryGet(1, out short key))
                    throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_key"]);
                if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(m_StringLocalizer["effectui_command:error_player"]);
                await UniTask.SwitchToMainThread();
                EffectManager.sendUIEffect(id, key, targetUser.Player.Player.channel.GetOwnerTransportConnection(), true);
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["effectui_command:prefix"] : "",
                    m_StringLocalizer["effectui_command:succeed:somebody:console", new
                    {
                        EffectID = id,
                        EffectKey = key
                    }]));
                PrintAsync(m_StringLocalizer["effectui_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        EffectID = id,
                        EffectKey = key
                    }]);
            }
        }
    }
}
