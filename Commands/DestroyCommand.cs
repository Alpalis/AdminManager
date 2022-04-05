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
    [Command("destroy")]
    [CommandDescription("Command to destroy object you're looking at.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class DestroyCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public DestroyCommand(
            IAdminSystem adminSystem,
            IStringLocalizer StringLocalizer,
            IConfigurationManager configurationManager,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = StringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 0)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : "",
                     m_StringLocalizer["destroy_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            PlayerLook look = user.Player.Player.look;
            if (!PhysicsUtility.raycast(new(look.getEyesPosition(), look.aim.forward),
                out var hit, 8f, RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE))
                throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : "",
                         m_StringLocalizer["destroy_command:error_null"]));
            InteractableVehicle vehicle = hit.collider.GetComponent<InteractableVehicle>();
            if (vehicle != null)
            {
                VehicleManager.askVehicleDestroy(vehicle);
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : ""),
                    m_StringLocalizer["destroy_command:succeed:vehicle"]));
                return;
            }
            BarricadeDrop bDrop = BarricadeManager.FindBarricadeByRootTransform(hit.transform);
            if (bDrop != null && BarricadeManager.tryGetRegion(hit.transform, out byte bx, out byte by, out ushort plant, out _))
            {
                BarricadeManager.destroyBarricade(bDrop, bx, by, plant);
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : ""),
                    m_StringLocalizer["destroy_command:succeed:barricade"]));
                return;
            }
            StructureDrop sDrop = StructureManager.FindStructureByRootTransform(hit.transform);
            if (sDrop != null && StructureManager.tryGetRegion(hit.transform, out byte sx, out byte sy, out _))
            {
                StructureManager.destroyStructure(sDrop, sx, sy, Vector3.zero);
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : ""),
                    m_StringLocalizer["destroy_command:succeed:structure"]));
                return;
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["destroy_command:prefix"] : "",
                     m_StringLocalizer["destroy_command:error_null"]));
        }

    }
}
