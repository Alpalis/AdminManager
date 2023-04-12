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
    public class VirusCommand
    {
        [Command("virus")]
        [CommandSyntax("[player]")]
        [CommandDescription("Allows to reset value of your or somebody's virus.")]
        [RegisterCommandPermission("other", Description = "Allows to reset vakye of other player's virus.")]
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
                         m_StringLocalizer["virus_command:prefix"],
                         m_StringLocalizer["virus_command:error_adminmode"]));
                if (Context.Parameters.Count == 0)
                {
                    await UniTask.SwitchToMainThread();
                    user.Player.Player.life.serverModifyVirus(100);
                    PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["virus_command:prefix"],
                        m_StringLocalizer["virus_command:yourself"]));
                    return;
                }
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        m_StringLocalizer["virus_command:prefix"],
                        m_StringLocalizer["virus_command:error_player"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                targetUser.Player.Player.life.serverModifyFood(100);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["virus_command:prefix"],
                    m_StringLocalizer["virus_command:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["virus_command:prefix"],
                    m_StringLocalizer["virus_command:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID
                    }]));
            }
        }

        [Command("virus")]
        [CommandSyntax("<player>")]
        [CommandDescription("Allows to reset value of somebody's virus.")]
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
                    throw new UserFriendlyException(m_StringLocalizer["virus_command:error_player"]);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await UniTask.SwitchToMainThread();
                user.Player.Player.life.serverModifyVirus(100);
                user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["virus_command:prefix"],
                    m_StringLocalizer["virus_command:somebody:console"]));
                PrintAsync(m_StringLocalizer["virus_command:somebody:executor", new
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
