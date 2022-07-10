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

namespace Alpalis.AdminManager.Commands
{
    public class VehicleCommand
    {
        #region Command Parameters
        [Command("vehicle")]
        [CommandAlias("v")]
        [CommandSyntax("<vehicle> [player]")]
        [CommandDescription("Command to spawn vehicles.")]
        [CommandActor(typeof(UnturnedUser))]
        [RegisterCommandPermission("other", Description = "Allows to spawn vehicles for other people.")]
        #endregion Command Parameters
        public class VehicleUnturned : UnturnedCommand
        {
            #region Member Variables
            private readonly IAdminSystem m_AdminSystem;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public VehicleUnturned(
                IAdminSystem adminSystem,
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_AdminSystem = adminSystem;
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count < 1 || Context.Parameters.Count > 2)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                UnturnedUser user = (UnturnedUser)Context.Actor;
                if (!m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                         m_StringLocalizer["vehicle_command:error_adminmode"]));
                if (!Context.Parameters.TryGet(0, out string? vehicleName) || vehicleName == null ||
                    !UnturnedAssetHelper.GetVehicle(vehicleName, out VehicleAsset vehicleAsset))
                    throw new UserFriendlyException(m_StringLocalizer["vehicle_command:error_null"]);
                await UniTask.SwitchToMainThread();
                if (Context.Parameters.Count == 1)
                {
                    if (!VehicleTool.giveVehicle(user.Player.Player, vehicleAsset.id))
                        throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                         m_StringLocalizer["vehicle_command:error_unknown"]));
                    PrintAsync(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                         m_StringLocalizer["vehicle_command:succeed:yourself",
                         new { VehicleName = vehicleAsset.vehicleName, VehicleID = vehicleAsset.id }]));
                    return;
                }
                if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                    throw new NotEnoughPermissionException(Context, "other");
                if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                        m_StringLocalizer["vehicle_command:error_player"]));
                if (!VehicleTool.giveVehicle(targetUser.Player.Player, vehicleAsset.id))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                     m_StringLocalizer["vehicle_command:error_unknown"]));
                SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
                CSteamID targetSteamID = targetSPlayer.playerID.steamID;
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                SteamPlayer sPlayer = user.Player.SteamPlayer;
                CSteamID steamID = sPlayer.playerID.steamID;
                ushort? identity = m_IdentityManagerImplementation.GetIdentity(steamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                    m_StringLocalizer["vehicle_command:succeed:somebody:player", new
                    {
                        PlayerName = sPlayer.playerID.playerName,
                        CharacterName = sPlayer.playerID.characterName,
                        NickName = sPlayer.playerID.nickName,
                        SteamID = steamID,
                        ID = identity,
                        VehicleName = vehicleAsset.vehicleName,
                        VehicleID = vehicleAsset.id
                    }]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                    m_StringLocalizer["vehicle_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        VehicleName = vehicleAsset.vehicleName,
                        VehicleID = vehicleAsset.id
                    }]));
            }
        }

        #region Command Parameters
        [Command("vehicle")]
        [CommandAlias("v")]
        [CommandSyntax("<vehicle> <player>")]
        [CommandDescription("Command to spawn vehicles.")]
        [CommandActor(typeof(ConsoleActor))]
        #endregion Command Parameters
        public class VehicleConsole : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public VehicleConsole(
                IStringLocalizer stringLocalizer,
                IIdentityManagerImplementation identityManagerImplementation,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_IdentityManagerImplementation = identityManagerImplementation;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 2)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
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
                ushort? targetIdentity = m_IdentityManagerImplementation.GetIdentity(targetSteamID);
                targetUser.PrintMessageAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["vehicle_command:prefix"] : "",
                    m_StringLocalizer["vehicle_command:succeed:somebody:console", new
                    {
                        VehicleName = vehicleAsset.vehicleName,
                        VehicleID = vehicleAsset.id
                    }]));
                PrintAsync(m_StringLocalizer["vehicle_command:succeed:somebody:executor", new
                    {
                        PlayerName = targetSPlayer.playerID.playerName,
                        CharacterName = targetSPlayer.playerID.characterName,
                        NickName = targetSPlayer.playerID.nickName,
                        SteamID = targetSteamID,
                        ID = targetIdentity,
                        VehicleName = vehicleAsset.vehicleName,
                        VehicleID = vehicleAsset.id
                    }]);
            }
        }
    }
}
