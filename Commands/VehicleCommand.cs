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

namespace Alpalis.AdminManager.Commands;

public sealed class VehicleCommand
{
    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandSyntax("<vehicle> [player]")]
    [CommandDescription("Spawn vehicles.")]
    [CommandActor(typeof(UnturnedUser))]
    [RegisterCommandPermission("other", Description = "Allows to spawn vehicles in front of other player.")]
    public sealed class Unturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count < 1 || Context.Parameters.Count > 2)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["vehicle_command:prefix"],
                     m_StringLocalizer["vehicle_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out string? vehicleName) || vehicleName == null ||
                !UnturnedAssetHelper.GetVehicle(vehicleName, out VehicleAsset vehicleAsset))
                throw new UserFriendlyException(m_StringLocalizer["vehicle_command:error_null"]);
            await UniTask.SwitchToMainThread();
            if (Context.Parameters.Count == 1)
            {
                if (!VehicleTool.giveVehicle(user.Player.Player, vehicleAsset.id))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["vehicle_command:prefix"],
                     m_StringLocalizer["vehicle_command:error_unknown"]));
                await PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["vehicle_command:prefix"],
                     m_StringLocalizer["vehicle_command:succeed:yourself",
                     new { VehicleName = vehicleAsset.vehicleName, VehicleID = vehicleAsset.id }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["vehicle_command:prefix"],
                    m_StringLocalizer["vehicle_command:error_player"]));
            if (!VehicleTool.giveVehicle(targetUser.Player.Player, vehicleAsset.id))
                throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["vehicle_command:prefix"],
                 m_StringLocalizer["vehicle_command:error_unknown"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["vehicle_command:prefix"],
                m_StringLocalizer["vehicle_command:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    VehicleName = vehicleAsset.vehicleName,
                    VehicleID = vehicleAsset.id
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["vehicle_command:prefix"],
                m_StringLocalizer["vehicle_command:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    VehicleName = vehicleAsset.vehicleName,
                    VehicleID = vehicleAsset.id
                }]));
        }
    }

    [Command("vehicle")]
    [CommandAlias("v")]
    [CommandSyntax("<vehicle> <player>")]
    [CommandDescription("Spawns vehicles.")]
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
            if (!Context.Parameters.TryGet(0, out string? vehicleName) || vehicleName == null ||
                !UnturnedAssetHelper.GetVehicle(vehicleName, out VehicleAsset vehicleAsset))
                throw new UserFriendlyException(m_StringLocalizer["vehicle_command:error_null"]);
            if (!Context.Parameters.TryGet(2, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["vehicle_command:error_player"]);
            await UniTask.SwitchToMainThread();
            if (!VehicleTool.giveVehicle(targetUser.Player.Player, vehicleAsset.id))
                throw new UserFriendlyException(m_StringLocalizer["vehicle_command:error_unknown"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["vehicle_command:prefix"],
                m_StringLocalizer["vehicle_command:succeed:somebody:console", new
                {
                    VehicleName = vehicleAsset.vehicleName,
                    VehicleID = vehicleAsset.id
                }]));
            await PrintAsync(m_StringLocalizer["vehicle_command:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                VehicleName = vehicleAsset.vehicleName,
                VehicleID = vehicleAsset.id
            }]);
        }
    }
}
