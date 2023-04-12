﻿using Alpalis.AdminManager.API;
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

namespace Alpalis.AdminManager.Commands.Movement
{
    public class GravityCommand
    {
        [Command("gravity")]
        [CommandSyntax("<multipler> [player]")]
        [CommandDescription("Sets gravity.")]
        [RegisterCommandPermission("other", Description = "Allows to set gravity of other player.")]
        [CommandActor(typeof(UnturnedUser))]
        public class Unturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IFlySystem m_FlySystem;

            public Unturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IFlySystem flySystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_FlySystem = flySystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["gravity_command:prefix"],
                         m_StringLocalizer["gravity_command:error_adminmode"]));
                if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["gravity_command:prefix"],
                        m_StringLocalizer["gravity_command:error_multipler"]));
                if (Context.Parameters.Count == 1)
                {
                    if (m_FlySystem.IsInFlyMode(user.SteamId))
                        throw new UserFriendlyException(string.Format("{0}{1}",
                            m_StringLocalizer["gravity_command:prefix"],
                            m_StringLocalizer["gravity_command:error_flymode:yourself"]));
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["gravity_command:prefix"],
                        m_StringLocalizer["gravity_command:yourself", new { Multipler = multipler }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["gravity_command:prefix"],
                        m_StringLocalizer["gravity_command:error_player"]));
                if (m_FlySystem.IsInFlyMode(targetUser.SteamId))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["gravity_command:prefix"],
                        m_StringLocalizer["gravity_command:error_flymode:somebody"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["gravity_command:prefix"],
                    m_StringLocalizer["gravity_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        Multipler = multipler
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["gravity_command:prefix"],
                    m_StringLocalizer["gravity_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        Multipler = multipler
                    }]));
            }
        }

        [Command("gravity")]
        [CommandSyntax("<multipler> <player>")]
        [CommandDescription("Sets gravity.")]
        [CommandActor(typeof(ConsoleActor))]
        public class Console : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IFlySystem m_FlySystem;

            public Console(
                IStringLocalizer stringLocalizer,
                IFlySystem flySystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_FlySystem = flySystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out float multipler))
                    throw new UserFriendlyException(m_StringLocalizer["gravity_command:error_multipler"]);
                if (!Context.Parameters.TryGet(1, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["gravity_command:error_player"]);
                if (m_FlySystem.IsInFlyMode(user.SteamId))
                    throw new UserFriendlyException(m_StringLocalizer["gravity_command:error_flymode:somebody"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                user.Player.Player.movement.sendPluginGravityMultiplier(multipler);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["gravity_command:prefix"],
                    m_StringLocalizer["gravity_command:somebody:console", new { Multipler = multipler }]));
                PrintAsync(m_StringLocalizer["gravity_command:somebody:executor", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Multipler = multipler
                }]);
            }
        }
    }
}
