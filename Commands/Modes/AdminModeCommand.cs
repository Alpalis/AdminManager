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

namespace Alpalis.AdminManager.Commands.Modes;

public sealed class AdminModeCommand
{
    [Command("adminmode")]
    [CommandSyntax("<get/switch>")]
    [CommandDescription("Manage adminmodes.")]
    public sealed class Root(
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }

    }

    [Command("switch")]
    [CommandSyntax("[player]")]
    [CommandDescription("Allows to turn on and off the adminmode.")]
    [RegisterCommandPermission("other", Description = "Allows to switch adminmode of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class SwitchUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IConfigurationManager configurationManager,
        IPluginAccessor<Main> plugin,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager = configurationManager;
        private readonly Main m_Plugin = plugin.Instance ?? throw new NullReferenceException("Plugin instance has not been found!");

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (config.DisableAdminMode)
            {
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"], m_StringLocalizer["adminmode_command:disabled"]));
            }
            if (Context.Parameters.Count == 0)
            {
                await UniTask.SwitchToMainThread();
                await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["adminmode_command:prefix"],
                    m_StringLocalizer[string.Format("adminmode_command:switch:yourself:{0}",
                    await m_AdminSystem.ToggleAdminMode(user) ? "enabled" : "disabled")]));
                return;
            }
            if (Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["adminmode_command:prefix"],
                     m_StringLocalizer["adminmode_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"],
                    m_StringLocalizer["adminmode_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            bool result = await m_AdminSystem.ToggleAdminMode(targetUser);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["adminmode_command:prefix"],
                m_StringLocalizer[string.Format("adminmode_command:switch:somebody:player:{0}",
                result ? "enabled" : "disabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID
                }]));
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["adminmode_command:prefix"],
                m_StringLocalizer[string.Format("adminmode_command:switch:somebody:executor:{0}",
                result ? "enabled" : "disabled"), new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID
                }]));
        }
    }

    [Command("switch")]
    [CommandSyntax("<player>")]
    [CommandDescription("Allows to turn on and off the adminmode of other player.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class SwitchConsole(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IConfigurationManager configurationManager,
        IPluginAccessor<Main> plugin,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager = configurationManager;
        private readonly Main m_Plugin = plugin.Instance ?? throw new NullReferenceException("Plugin instance has not been found!");

        protected override async UniTask OnExecuteAsync()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (config.DisableAdminMode)
            {
                throw new UserFriendlyException(m_StringLocalizer["adminmode_command:disabled"]);
            }
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"],
                    m_StringLocalizer["adminmode_command:error_player"]));
            await UniTask.SwitchToMainThread();
            bool result = await m_AdminSystem.ToggleAdminMode(user);
            await user.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["adminmode_command:prefix"],
                m_StringLocalizer[string.Format("adminmode_command:switch:somebody:console:{0}",
                result ? "enabled" : "disabled")]));
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await PrintAsync(m_StringLocalizer[string.Format("adminmode_command:switch:somebody:executor:{0}", result ? "enabled" : "disabled"), new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID
            }]);
        }
    }

    [Command("get")]
    [CommandSyntax("[player]")]
    [CommandDescription("Allows to get state of your or somebody's adminmode.")]
    [RegisterCommandPermission("other", Description = "Allows to get adminmode of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class GetUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IConfigurationManager configurationManager,
        IPluginAccessor<Main> plugin,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager = configurationManager;
        private readonly Main m_Plugin = plugin.Instance ?? throw new NullReferenceException("Plugin instance has not been found!");

        protected override async UniTask OnExecuteAsync()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (config.DisableAdminMode)
            {
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"], m_StringLocalizer["adminmode_command:disabled"]));
            }
            UnturnedUser user = (UnturnedUser)Context.Actor;
            bool result = m_AdminSystem.IsInAdminMode(user.SteamId);
            if (Context.Parameters.Length == 0)
            {
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"],
                    m_StringLocalizer[string.Format("adminmode_command:get:yourself:{0}", result ? "enabled" : "disabled")]));
                return;
            }
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!result)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["adminmode_command:prefix"],
                     m_StringLocalizer["adminmode_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["adminmode_command:prefix"],
                    m_StringLocalizer["adminmode_command:error_player"]));
            SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            bool resultTarget = m_AdminSystem.IsInAdminMode(steamID);
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["adminmode_command:prefix"],
                m_StringLocalizer[string.Format("adminmode_command:get:somebody:{0}", resultTarget ? "enabled" : "disabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID
                }]));
        }
    }

    [Command("get")]
    [CommandSyntax("<player>")]
    [CommandDescription("Allows to get state of somebody's adminmode.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class GetConsole(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IConfigurationManager configurationManager,
        IPluginAccessor<Main> pluginAccessor,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager = configurationManager;
        private readonly Main m_Plugin = pluginAccessor.Instance ?? throw new NullReferenceException("Plugin instance has not been found!");

        protected override async UniTask OnExecuteAsync()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            if (config.DisableAdminMode)
            {
                throw new UserFriendlyException(m_StringLocalizer["adminmode_command:disabled"]);
            }
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["adminmode_command:error_player"]);
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            bool result = m_AdminSystem.IsInAdminMode(steamID);
            await PrintAsync(m_StringLocalizer[string.Format("adminmode_command:get:somebody:{0}", result ? "enabled" : "disabled"), new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID
            }]);
        }
    }
}
