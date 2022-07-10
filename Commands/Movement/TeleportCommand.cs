using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.Helpers;
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
using UnityEngine;

namespace Alpalis.AdminManager.Commands.Movement
{
    public class TeleportCommand
    {
        #region Command Parameters
        [Command("teleport")]
        [CommandAlias("tp")]
        [CommandSyntax("<player> <player/place>")]
        [CommandDescription("Command to teleport player to another player or place.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class TeleportConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TeleportConsole(
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
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                    throw new UserFriendlyException(m_StringLocalizer["teleport_command:error_player"]);
                if (Context.Parameters.TryGet(1, out UnturnedUser? targetUser) && targetUser != null)
                {
                    user.Player.Player.TeleportToLocationAsync(targetUser.Player.Player.transform.position);
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                    SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                    CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                    ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                    user.PrintMessageAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:somebody:succeed:console:player", new
                        {
                            TargetPlayerName = targetSPlayer.playerID.playerName,
                            TargetCharacterName = targetSPlayer.playerID.characterName,
                            TargetNickName = targetSPlayer.playerID.nickName,
                            TargetSteamID = targetSteamID,
                            TargetID = targetIdentity
                        }]));
                    PrintAsync(m_StringLocalizer["teleport_command:somebody:succeed:executor:player", new
                    {
                        TargetPlayerName = targetSPlayer.playerID.playerName,
                        TargetCharacterName = targetSPlayer.playerID.characterName,
                        TargetNickName = targetSPlayer.playerID.nickName,
                        TargetSteamID = targetSteamID,
                        TargetID = targetIdentity,
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity
                    }]);
                    return;
                }
                else if (Context.Parameters.TryGet(1, out string? place) && place != null
                    && LocationNodeHelper.TryGetLocationNode(place, out LocationNode outNode))
                {
                    user.Player.Player.TeleportToLocationAsync(outNode.point);
                    user.PrintMessageAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:somebody:succeed:console:place", new
                        {
                            Location = outNode.name
                        }]));
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                    PrintAsync(m_StringLocalizer["teleport_command:somebody:succeed:executor:place", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        Location = outNode.name
                    }]);
                    return;
                }
                    throw new UserFriendlyException(m_StringLocalizer["teleport_command:somebody:error_null"]);
            }
        }

        #region Command Parameters
        [Command("teleport")]
        [CommandAlias("tp")]
        [CommandSyntax("[player] <player/place/marker>")]
        [CommandDescription("Command to teleport youself or player to another player or place.")]
        [RegisterCommandPermission("other", Description = "Allows to teleport other player.")]
        [CommandActor(typeof(UnturnedUser))]
        #endregion Command Parameters
        public class TeleportUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public TeleportUnturned(
                IAdminSystem adminSystem,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IStringLocalizer stringLocalizer,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
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
                         config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                         m_StringLocalizer["teleport_command:error_adminmode"]));
                if (Context.Parameters.Count != 2 && Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (Context.Parameters.Count == 1)
                {
                    if (Context.Parameters.TryGet(0, out UnturnedUser? targetUserYourself) && targetUserYourself != null)
                    {
                        SteamPlayer sPlayer = targetUserYourself.Player.SteamPlayer;
                        CSteamID steamID = sPlayer.playerID.steamID;
                        ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                        user.Player.Player.TeleportToLocationAsync(targetUserYourself.Player.Player.transform.position);
                        PrintAsync(string.Format("{0}{1}",
                            config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                            m_StringLocalizer["teleport_command:yourself:succeed:player", new
                            {
                                PlayerName = sPlayer.playerID.playerName,
                                CharacterName = sPlayer.playerID.characterName,
                                NickName = sPlayer.playerID.nickName,
                                SteamID = steamID,
                                ID = identity
                            }]));
                        return;
                    }
                    else if (Context.Parameters.TryGet(0, out string? place) && place != null)
                    {
                        if (place == "marker")
                        {
                            if (!user.Player.Player.quests.isMarkerPlaced)
                                throw new UserFriendlyException(string.Format("{0}{1}",
                                    config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                                    m_StringLocalizer["teleport_command:error_marker"]));
                            Vector3 position = user.Player.Player.quests.markerPosition;
                            position.y = 1024f;
                            if (Physics.Raycast(position, Vector3.down, out RaycastHit raycastHit, 2048f, RayMasks.WAYPOINT))
                                position = raycastHit.point + Vector3.up;
                            user.Player.Player.TeleportToLocationAsync(position);
                            PrintAsync(string.Format("{0}{1}",
                                config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                                m_StringLocalizer["teleport_command:yourself:succeed:marker"]));
                            return;
                        }
                        else if (LocationNodeHelper.TryGetLocationNode(place, out LocationNode outNode))
                        {
                            user.Player.Player.TeleportToLocationAsync(outNode.point);
                            PrintAsync(string.Format("{0}{1}",
                                config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                                m_StringLocalizer["teleport_command:yourself:succeed:place", new
                                {
                                    Location = outNode.name
                                }]));
                            return;
                        }
                    }
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:yourself:error_null"]));
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(0, out UnturnedUser? teleportUser) || teleportUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:error_player"]));
                if (Context.Parameters.TryGet(1, out UnturnedUser? targetUser) && targetUser != null)
                {
                    SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                    CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                    ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                    SteamPlayer sPlayer = user.Player.SteamPlayer;
                    CSteamID steamID = sPlayer.playerID.steamID;
                    ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                    SteamPlayer teleportSPlayer = teleportUser.Player.SteamPlayer;
                    CSteamID teleportSteamID = teleportSPlayer.playerID.steamID;
                    ushort? teleportIdentity = m_IdentityManagerImplementation.GetIdentity(teleportSteamID);
                    teleportUser.Player.Player.TeleportToLocationAsync(targetUser.Player.Player.transform.position);
                    teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:somebody:succeed:player:player", new
                        {
                            TargetPlayerName = targetSPlayer.playerID.playerName,
                            TargetCharacterName = targetSPlayer.playerID.characterName,
                            TargetNickName = targetSPlayer.playerID.nickName,
                            TargetSteamID = targetSteamID,
                            TargetID = targetIdentity,
                            PlayerName = sPlayer.playerID.playerName,
                            CharacterName = sPlayer.playerID.characterName,
                            NickName = sPlayer.playerID.nickName,
                            SteamID = steamID,
                            ID = identity
                        }]));
                    PrintAsync(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                        m_StringLocalizer["teleport_command:somebody:succeed:executor:player", new
                        {
                            TargetPlayerName = targetSPlayer.playerID.playerName,
                            TargetCharacterName = targetSPlayer.playerID.characterName,
                            TargetNickName = targetSPlayer.playerID.nickName,
                            TargetSteamID = targetSteamID,
                            TargetID = targetIdentity,
                            PlayerName = teleportSPlayer.playerID.playerName,
                            CharacterName = teleportSPlayer.playerID.characterName,
                            NickName = teleportSPlayer.playerID.nickName,
                            SteamID = teleportSteamID,
                            ID = teleportIdentity
                        }]));
                    return;
                }
                else if (Context.Parameters.TryGet(1, out string? place) && place != null)
                {
                    if (place == "marker")
                    {
                        if (!user.Player.Player.quests.isMarkerPlaced)
                            throw new UserFriendlyException(string.Format("{0}{1}",
                                config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                                m_StringLocalizer["teleport_command:error_marker"]));
                        Vector3 position = user.Player.Player.quests.markerPosition;
                        position.y = 1024f;
                        if (Physics.Raycast(position, Vector3.down, out RaycastHit raycastHit, 2048f, RayMasks.WAYPOINT))
                            position = raycastHit.point + Vector3.up;
                        teleportUser.Player.Player.TeleportToLocationAsync(position);
                        SteamPlayer sPlayer = user.Player.SteamPlayer;
                        CSteamID steamID = sPlayer.playerID.steamID;
                        ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                        teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                            config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                            m_StringLocalizer["teleport_command:somebody:succeed:player:marker", new
                            {
                                PlayerName = sPlayer.playerID.playerName,
                                CharacterName = sPlayer.playerID.characterName,
                                NickName = sPlayer.playerID.nickName,
                                SteamID = steamID,
                                ID = identity
                            }]));
                        PrintAsync(string.Format("{0}{1}",
                            config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                            m_StringLocalizer["teleport_command:somebody:succeed:executor:marker", new
                            {
                                PlayerName = sPlayer.playerID.playerName,
                                CharacterName = sPlayer.playerID.characterName,
                                NickName = sPlayer.playerID.nickName,
                                SteamID = steamID,
                                ID = identity
                            }]));
                        return;
                    }
                    else if (LocationNodeHelper.TryGetLocationNode(place, out LocationNode outNode))
                    {
                        teleportUser.Player.Player.TeleportToLocationAsync(outNode.point);
                        SteamPlayer sPlayer = user.Player.SteamPlayer;
                        CSteamID steamID = sPlayer.playerID.steamID;
                        ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                        teleportUser.PrintMessageAsync(string.Format("{0}{1}",
                            config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                            m_StringLocalizer["teleport_command:somebody:succeed:player:place", new
                            {
                                PlayerName = sPlayer.playerID.playerName,
                                CharacterName = sPlayer.playerID.characterName,
                                NickName = sPlayer.playerID.nickName,
                                SteamID = steamID,
                                ID = identity,
                                Location = outNode.name
                            }]));
                        PrintAsync(string.Format("{0}{1}",
                            config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                            m_StringLocalizer["teleport_command:somebody:succeed:executor:place", new
                            {
                                PlayerName = sPlayer.playerID.playerName,
                                CharacterName = sPlayer.playerID.characterName,
                                NickName = sPlayer.playerID.nickName,
                                SteamID = steamID,
                                ID = identity,
                                Location = outNode.name
                            }]));
                        return;
                    }
                }
                throw new UserFriendlyException(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["teleport_command:prefix"] : "",
                    m_StringLocalizer["teleport_command:somebody:error_null"]));
            }
        }
    }
}
