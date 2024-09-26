using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands.Modes;

public sealed class GodCommand
{
    [Command("god")]
    [CommandSyntax("<get/switch>")]
    [CommandDescription("Manage godmodes.")]
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
    [CommandDescription("Allows to turn on and off the godmode.")]
    [RegisterCommandPermission("other", Description = "Allows to switch godmode of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class SwitchUnturned(
        IGodSystem godSystem,
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IGodSystem m_GodSystem = godSystem;
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["god_command:prefix"],
                     m_StringLocalizer["god_command:error_adminmode"]));
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            if (Context.Parameters.Length == 0)
            {
                bool result = m_GodSystem.IsInGodMode(user.SteamId);
                await UniTask.SwitchToMainThread();
                if (result)
                    await m_GodSystem.DisableGodMode(sPlayer);
                else
                    await m_GodSystem.EnableGodMode(sPlayer);
                await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer[string.Format("god_command:switch:yourself:{0}",
                    result ? "disabled" : "enabled")]));
                return;
            }
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer["god_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            bool targetResult = m_GodSystem.IsInGodMode(targetSteamID);
            await UniTask.SwitchToMainThread();
            if (targetResult)
                await m_GodSystem.DisableGodMode(targetSPlayer);
            else
                await m_GodSystem.EnableGodMode(targetSPlayer);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                m_StringLocalizer[string.Format("god_command:switch:somebody:player:{0}",
                targetResult ? "disabled" : "enabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = user.SteamId
                }]));
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                m_StringLocalizer[string.Format("god_command:switch:somebody:executor:{0}",
                targetResult ? "disabled" : "enabled"), new
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
    [CommandDescription("Allows to turn on and off the godmode of other player.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class SwitchConsole(
        IGodSystem godSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IGodSystem m_GodSystem = godSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer["god_command:error_player"]));
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            bool result = m_GodSystem.IsInGodMode(user.SteamId);
            await UniTask.SwitchToMainThread();
            if (result)
                await m_GodSystem.DisableGodMode(sPlayer);
            else
                await m_GodSystem.EnableGodMode(sPlayer);
            await user.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                m_StringLocalizer[string.Format("god_command:switch:somebody:console:{0}",
                result ? "disabled" : "enabled")]));
            await PrintAsync(m_StringLocalizer[string.Format("god_command:switch:somebody:executor{0}", result ? "disabled" : "enabled"), new
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
    [CommandDescription("Allows to get state of your or somebody's godmode.")]
    [RegisterCommandPermission("other", Description = "Allows to get godmode state of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class GetUnturned(
        IAdminSystem adminSystem,
        IGodSystem godSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IGodSystem m_GodSystem = godSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["god_command:prefix"],
                     m_StringLocalizer["god_command:error_adminmode"]));
            if (Context.Parameters.Length == 0)
            {
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer[string.Format("god_command:get:yourself:{0}",
                    m_GodSystem.IsInGodMode(user.SteamId) ? "enabled" : "disabled")]));
                return;
            }
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer["god_command:error_player"]));
            SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                m_StringLocalizer[string.Format("god_command:get:somebody:{0}",
                m_GodSystem.IsInGodMode(steamID) ? "enabled" : "disabled"), new
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
    [CommandDescription("Command to get state of somebody's god modes.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class GetConsole(
        IGodSystem godSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IGodSystem m_GodSystem = godSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["god_command:error_player"]);
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            bool result = m_GodSystem.IsInGodMode(steamID);
            await PrintAsync(m_StringLocalizer[string.Format("god_command:get:somebody:{0}", result ? "enabled" : "disabled"), new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID
            }]);
        }
    }
}
