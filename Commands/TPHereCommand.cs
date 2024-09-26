using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands;

[Command("tphere")]
[CommandDescription("Teleports player to you.")]
[CommandSyntax("<player>")]
[CommandActor(typeof(UnturnedUser))]
public sealed class TPHereCommand(
    IAdminSystem adminSystem,
    IStringLocalizer stringLocalizer,
    IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
{
    private readonly IAdminSystem m_AdminSystem = adminSystem;
    private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

    protected override async UniTask OnExecuteAsync()
    {
        if (Context.Parameters.Count != 1)
            throw new CommandWrongUsageException(Context);
        UnturnedUser user = (UnturnedUser)Context.Actor;
        if (!m_AdminSystem.IsInAdminMode(user))
            throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["tphere_command:prefix"],
                 m_StringLocalizer["tphere_command:error_adminmode"]));
        if (!Context.Parameters.TryGet(0, out UnturnedUser? teleportUser) || teleportUser == null)
            throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["tphere_command:prefix"],
                m_StringLocalizer["tphere_command:error_player"]));
        SteamPlayer sPlayer = user.Player.SteamPlayer;
        CSteamID steamID = sPlayer.playerID.steamID;
        SteamPlayer teleportSPlayer = teleportUser.Player.SteamPlayer;
        CSteamID teleportSteamID = teleportSPlayer.playerID.steamID;
        await teleportUser.Player.Player.TeleportToLocationAsync(user.Player.Player.transform.position);
        await teleportUser.PrintMessageAsync(string.Format("{0}{1}",
            m_StringLocalizer["tphere_command:prefix"],
            m_StringLocalizer["tphere_command:succeed:player", new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID
            }]));
        await PrintAsync(string.Format("{0}{1}",
            m_StringLocalizer["tphere_command:prefix"],
            m_StringLocalizer["tphere_command:succeed:executor", new
            {
                PlayerName = teleportSPlayer.playerID.playerName,
                CharacterName = teleportSPlayer.playerID.characterName,
                NickName = teleportSPlayer.playerID.nickName,
                SteamID = teleportSteamID
            }]));
    }
}
