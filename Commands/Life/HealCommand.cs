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

namespace Alpalis.AdminManager.Commands.Life
{
    public class HealCommand
    {
        [Command("heal")]
        [CommandSyntax("[player]")]
        [CommandDescription("Allows to set max value of your or somebody's health.")]
        [RegisterCommandPermission("other", Description = "Allows to set max value of other player's health.")]
        [CommandActor(typeof(UnturnedUser))]
        public class Unturned : UnturnedCommand
        {
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;

            public Unturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["heal_command:prefix"],
                         m_StringLocalizer["heal_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.life.serverModifyHealth(100);
                    user.Player.Player.life.serverSetBleeding(false);
                    user.Player.Player.life.serverSetLegsBroken(false);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["heal_command:prefix"],
                        m_StringLocalizer["heal_command:yourself"]));
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["heal_command:prefix"],
                        m_StringLocalizer["heal_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.life.serverModifyHealth(100);
                targetUser.Player.Player.life.serverSetBleeding(false);
                targetUser.Player.Player.life.serverSetLegsBroken(false);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["heal_command:prefix"],
                    m_StringLocalizer["heal_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["heal_command:prefix"],
                    m_StringLocalizer["heal_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID
                    }]));
            }
        }

        [Command("heal")]
        [CommandSyntax("<player>")]
        [CommandDescription("Allows to set max value of somebody's health.")]
        [CommandActor(typeof(ConsoleActor))]
        public class Console : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;

            public Console(
                IStringLocalizer stringLocalizer,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["heal_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                user.Player.Player.life.serverModifyHealth(100);
                user.Player.Player.life.serverSetBleeding(false);
                user.Player.Player.life.serverSetLegsBroken(false);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["heal_command:prefix"],
                    m_StringLocalizer["heal_command:somebody:console"]));
                PrintAsync(m_StringLocalizer["heal_command:somebody:executor", new
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
