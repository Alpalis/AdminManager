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
    [Command("owner")]
    [CommandDescription("Command to check owner of object you're looking at.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class OwnerCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public OwnerCommand(
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
                     config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : "",
                     m_StringLocalizer["owner_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            PlayerLook look = user.Player.Player.look;
            if (!PhysicsUtility.raycast(new(look.getEyesPosition(), look.aim.forward),
                out var hit, 8f, RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE))
                throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : "",
                         m_StringLocalizer["owner_command:error_null"]));
            InteractableVehicle vehicle = hit.collider.GetComponent<InteractableVehicle>();
            if (vehicle != null)
            {
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : ""),
                    m_StringLocalizer["owner_command:succeed:vehicle", new
                    {
                        SteamID = vehicle.lockedOwner
                    }]));
                return;
            }
            BarricadeDrop bDrop = BarricadeManager.FindBarricadeByRootTransform(hit.transform);
            if (bDrop != null)
            {
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : ""),
                    m_StringLocalizer["owner_command:succeed:barricade", new
                    {
                        SteamID = bDrop.GetServersideData().owner
                    }]));
                return;
            }
            StructureDrop sDrop = StructureManager.FindStructureByRootTransform(hit.transform);
            if (sDrop != null)
            {
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : ""),
                    m_StringLocalizer["owner_command:succeed:structure", new
                    {
                        SteamID = sDrop.GetServersideData().owner
                    }]));
                return;
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["owner_command:prefix"] : "",
                     m_StringLocalizer["owner_command:error_null"]));
        }

    }
}
