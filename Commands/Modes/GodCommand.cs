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

namespace Alpalis.AdminManager.Commands.Modes
{
    public class GodCommand
    {
        [Command("god")]
        [CommandSyntax("<get/switch>")]
        [CommandDescription("Manage godmodes.")]
        public class Root : UnturnedCommand
        {
            public Root(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
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
        public class SwitchUnturned : UnturnedCommand
        {
            private readonly IGodSystem m_GodSystem;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public SwitchUnturned(
                IGodSystem godSystem,
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_GodSystem = godSystem;
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

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
                        m_GodSystem.DisableGodMode(sPlayer);
                    else
                        m_GodSystem.EnableGodMode(sPlayer);
                    PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
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
                    m_GodSystem.DisableGodMode(targetSPlayer);
                else
                    m_GodSystem.EnableGodMode(targetSPlayer);
                targetUser.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer[string.Format("god_command:switch:somebody:player:{0}",
                    targetResult ? "disabled" : "enabled"), new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = user.SteamId
                    }]));
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
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
        public class SwitchConsole : UnturnedCommand
        {
            private readonly IGodSystem m_GodSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public SwitchConsole(
                IGodSystem godSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_GodSystem = godSystem;
                m_StringLocalizer = stringLocalizer;
            }

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
                    m_GodSystem.DisableGodMode(sPlayer);
                else
                    m_GodSystem.EnableGodMode(sPlayer);
                user.PrintMessageAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
                    m_StringLocalizer[string.Format("god_command:switch:somebody:console:{0}",
                    result ? "disabled" : "enabled")]));
                PrintAsync(m_StringLocalizer[string.Format("god_command:switch:somebody:executor{0}", result ? "disabled" : "enabled"), new
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
        public class GetUnturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IGodSystem m_GodSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public GetUnturned(
                IAdminSystem adminSystem,
                IGodSystem godSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_GodSystem = godSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["god_command:prefix"],
                         m_StringLocalizer["god_command:error_adminmode"]));
                if (Context.Parameters.Length == 0)
                {
                    PrintAsync(string.Format("{0}{1}",
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
                PrintAsync(string.Format("{0}{1}", m_StringLocalizer["god_command:prefix"],
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
        public class GetConsole : UnturnedCommand
        {
            private readonly IGodSystem m_GodSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public GetConsole(
                IGodSystem godSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_GodSystem = godSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["god_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                bool result = m_GodSystem.IsInGodMode(steamID);
                PrintAsync(m_StringLocalizer[string.Format("god_command:get:somebody:{0}", result ? "enabled" : "disabled"), new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID
                }]);
            }
        }
    }
}
