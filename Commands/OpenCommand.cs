using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Plugins;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Framework.Utilities;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Alpalis.AdminManager.Commands
{
    #region Commad Parameters
    [Command("open")]
    [CommandDescription("Command to force open door, storage and vehicles.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class OpenCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public OpenCommand(
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                     m_StringLocalizer["open_command:error_adminmode"]));
            PlayerLook look = user.Player.Player.look;
            await UniTask.SwitchToMainThread();
            Transform aim = user.Player.Player.look.aim;
            RaycastInfo raycast = DamageTool.raycast(new(aim.position, aim.forward), 8f, RayMasks.BARRICADE | RayMasks.VEHICLE);
            if (raycast == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                    m_StringLocalizer["open_command:error_null"]));
            Interactable interactable = raycast.collider.GetComponent<Interactable>();
            if (interactable is InteractableDoorHinge hinge and { door: not null })
            {
                BarricadeManager.ServerSetDoorOpen(hinge.door, !hinge.door.isOpen);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                    m_StringLocalizer[string.Format("open_command:succeed:door:{0}", hinge.door.isOpen ? "open" : "close")]));
                return;
            }
            else if (interactable is InteractableStorage storage)
            {
                user.Player.Player.inventory.openStorage(storage);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                    m_StringLocalizer["open_command:succeed:storage"]));
                return;
            }
            else if (interactable is InteractableVehicle vehicle)
            {
                var shouldLock = !vehicle.isLocked;
                var owner = shouldLock ? user.SteamId : vehicle.lockedOwner;
                var group = shouldLock ? user.Player.Player.quests.groupID : vehicle.lockedGroup;
                VehicleManager.ServerSetVehicleLock(vehicle, owner, group, shouldLock);
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                    m_StringLocalizer[string.Format("open_command:succeed:vehicle:{0}", shouldLock ? "close" : "open")]));
                return;
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                config.MessagePrefix ? m_StringLocalizer["open_command:prefix"] : "",
                m_StringLocalizer["open_command:error_null"]));
        }
    }
}
