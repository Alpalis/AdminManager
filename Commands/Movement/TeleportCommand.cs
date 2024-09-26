using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.Helpers;
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
using UnityEngine;

namespace Alpalis.AdminManager.Commands.Movement;

public sealed class TeleportCommand
{
    [Command("teleport")]
    [CommandAlias("tp")]
    [CommandSyntax("<player> <player/place>")]
    [CommandDescription("Teleports player to another player or place.")]
    [CommandActor(typeof(ConsoleActor))]
    public sealed class Console(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["teleport_command:error_player"]);
            if (Context.Parameters.TryGet(1, out UnturnedUser? targetUser) && targetUser != null)
            {
                await user.Player.Player.TeleportToLocationAsync(targetUser.Player.Player.transform.position);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                await user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:somebody:succeed:console:player", new
                    {
                        TargetPlayerName = targetSPlayer.playerID.playerName,
                        TargetCharacterName = targetSPlayer.playerID.characterName,
                        TargetNickName = targetSPlayer.playerID.nickName,
                        TargetSteamID = targetSteamID
                    }]));
                await PrintAsync(m_StringLocalizer["teleport_command:somebody:succeed:executor:player", new
                {
                    TargetPlayerName = targetSPlayer.playerID.playerName,
                    TargetCharacterName = targetSPlayer.playerID.characterName,
                    TargetNickName = targetSPlayer.playerID.nickName,
                    TargetSteamID = targetSteamID,
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID
                }]);
                return;
            }
            else if (Context.Parameters.TryGet(1, out string? place) && place != null
                && LocationNodeHelper.TryGetLocationNode(place, out LocationDevkitNode? outNode)
                && outNode != null)
            {
                await user.Player.Player.TeleportToLocationAsync(outNode.inspectablePosition);
                await user.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:somebody:succeed:console:place", new
                    {
                        Location = outNode.name
                    }]));
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                await PrintAsync(m_StringLocalizer["teleport_command:somebody:succeed:executor:place", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Location = outNode.name
                }]);
                return;
            }
            throw new UserFriendlyException(m_StringLocalizer["teleport_command:somebody:error_null"]);
        }
    }

    [Command("teleport")]
    [CommandAlias("tp")]
    [CommandSyntax("[player] <player/place/marker>")]
    [CommandDescription("Teleports player to another player, place or marker.")]
    [RegisterCommandPermission("other", Description = "Allows to teleport other player.")]
    [CommandActor(typeof(UnturnedUser))]
    public sealed class Unturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["teleport_command:prefix"],
                     m_StringLocalizer["teleport_command:error_adminmode"]));
            if (Context.Parameters.Count != 2 && Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            if (Context.Parameters.Count == 1)
            {
                if (Context.Parameters.TryGet(0, out UnturnedUser? targetUserYourself) && targetUserYourself != null)
                {
                    SteamPlayer sPlayer = targetUserYourself.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    await user.Player.Player.TeleportToLocationAsync(targetUserYourself.Player.Player.transform.position);
                    await PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["teleport_command:prefix"],
                        m_StringLocalizer["teleport_command:yourself:succeed:player", new
                        {
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID
                        }]));
                    return;
                }
                else if (Context.Parameters.TryGet(0, out string? place) && place != null)
                {
                    if (place == "marker")
                    {
                        if (!user.Player.Player.quests.isMarkerPlaced)
                            throw new UserFriendlyException(string.Format("{0}{1}",
                                m_StringLocalizer["teleport_command:prefix"],
                                m_StringLocalizer["teleport_command:error_marker"]));
                        Vector3 position = user.Player.Player.quests.markerPosition;
                        position.y = 1024f;
                        if (Physics.Raycast(position, Vector3.down, out RaycastHit raycastHit, 2048f, RayMasks.WAYPOINT))
                            position = raycastHit.point + Vector3.up;
                        await user.Player.Player.TeleportToLocationAsync(position);
                        await PrintAsync(string.Format("{0}{1}",
                            m_StringLocalizer["teleport_command:prefix"],
                            m_StringLocalizer["teleport_command:yourself:succeed:marker"]));
                        return;
                    }
                    else if (LocationNodeHelper.TryGetLocationNode(place, out LocationDevkitNode? outNode)
                        && outNode != null)
                    {
                        await user.Player.Player.TeleportToLocationAsync(outNode.inspectablePosition);
                        await PrintAsync(string.Format("{0}{1}",
                            m_StringLocalizer["teleport_command:prefix"],
                            m_StringLocalizer["teleport_command:yourself:succeed:place", new
                            {
                                Location = outNode.name
                            }]));
                        return;
                    }
                }
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:yourself:error_null"]));
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(0, out UnturnedUser? teleportUser) || teleportUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:error_player"]));
            if (Context.Parameters.TryGet(1, out UnturnedUser? targetUser) && targetUser != null)
            {
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                SteamPlayer teleportSPlayer = teleportUser.Player.SteamPlayer;
                CSteamID teleportSteamID = teleportSPlayer.playerID.steamID;
                await teleportUser.Player.Player.TeleportToLocationAsync(targetUser.Player.Player.transform.position);
                await teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:somebody:succeed:player:player", new
                    {
                        TargetPlayerName = targetSPlayer.playerID.playerName,
                        TargetCharacterName = targetSPlayer.playerID.characterName,
                        TargetNickName = targetSPlayer.playerID.nickName,
                        TargetSteamID = targetSteamID,
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID
                    }]));
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["teleport_command:prefix"],
                    m_StringLocalizer["teleport_command:somebody:succeed:executor:player", new
                    {
                        TargetPlayerName = targetSPlayer.playerID.playerName,
                        TargetCharacterName = targetSPlayer.playerID.characterName,
                        TargetNickName = targetSPlayer.playerID.nickName,
                        TargetSteamID = targetSteamID,
                        PlayerName = teleportSPlayer.playerID.playerName,
                        CharacterName = teleportSPlayer.playerID.characterName,
                        NickName = teleportSPlayer.playerID.nickName,
                        SteamID = teleportSteamID
                    }]));
                return;
            }
            else if (Context.Parameters.TryGet(1, out string? place) && place != null)
            {
                if (place == "marker")
                {
                    if (!user.Player.Player.quests.isMarkerPlaced)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                            m_StringLocalizer["teleport_command:prefix"],
                            m_StringLocalizer["teleport_command:error_marker"]));
                    Vector3 position = user.Player.Player.quests.markerPosition;
                    position.y = 1024f;
                    if (Physics.Raycast(position, Vector3.down, out RaycastHit raycastHit, 2048f, RayMasks.WAYPOINT))
                        position = raycastHit.point + Vector3.up;
                    await teleportUser.Player.Player.TeleportToLocationAsync(position);
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    await teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                        m_StringLocalizer["teleport_command:prefix"],
                        m_StringLocalizer["teleport_command:somebody:succeed:player:marker", new
                        {
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID
                        }]));
                    await PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["teleport_command:prefix"],
                        m_StringLocalizer["teleport_command:somebody:succeed:executor:marker", new
                        {
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID
                        }]));
                    return;
                }
                else if (LocationNodeHelper.TryGetLocationNode(place, out LocationDevkitNode? outNode)
                    && outNode != null)
                {
                    await teleportUser.Player.Player.TeleportToLocationAsync(outNode.inspectablePosition);
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    await teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                        m_StringLocalizer["teleport_command:prefix"],
                        m_StringLocalizer["teleport_command:somebody:succeed:player:place", new
                        {
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID,
                            Location = outNode.name
                        }]));
                    await PrintAsync(string.Format("{0}{1}",
                        m_StringLocalizer["teleport_command:prefix"],
                        m_StringLocalizer["teleport_command:somebody:succeed:executor:place", new
                        {
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID,
                            Location = outNode.name
                        }]));
                    return;
                }
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                m_StringLocalizer["teleport_command:prefix"],
                m_StringLocalizer["teleport_command:somebody:error_null"]));
        }
    }
}
