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
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    #region Commad Parameters
    [Command("refuel")]
    [CommandDescription("Command to refuel the object you're looking at or current vehicle.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class RefuelCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public RefuelCommand(
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
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            InteractableVehicle currentVehicle = user.Player.Player.movement.getVehicle();
            if (currentVehicle != null)
            {
                if (!RefuelVehicle(currentVehicle))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_fuel:vehicle", new { Vehicle = currentVehicle.name }]));
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                    m_StringLocalizer["refuel_command:succeed:vehicle:current", new { Vehicle = currentVehicle.name }]));
                return;
            }
            PlayerLook look = user.Player.Player.look;
            RaycastInfo raycast = DamageTool.raycast(new(look.aim.position, look.aim.forward), 8f, RayMasks.DAMAGE_SERVER | RayMasks.VEHICLE);
            if (raycast.vehicle != null)
            {
                if (!RefuelVehicle(raycast.vehicle))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_fuel:vehicle", new { Vehicle = raycast.vehicle.name }]));
                PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                    m_StringLocalizer["refuel_command:succeed:vehicle:looking_at", new { Vehicle = raycast.vehicle.name }]));
                return;
            }
            if (raycast.transform == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                         m_StringLocalizer["refuel_command:error_null"]));
            Interactable interactable = raycast.transform.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable is InteractableGenerator generator)
                {
                    if (!generator.isRefillable)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_fuel:object", new { Object = generator.name }]));
                    generator.askFill(generator.capacity);
                    BarricadeManager.sendFuel(raycast.transform, generator.fuel);
                    PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                        m_StringLocalizer["refuel_command:succeed:generator", new { Object = generator.name }]));
                    return;
                }
                else if (interactable is InteractableOil oil)
                {
                    if (!oil.isRefillable)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                        m_StringLocalizer["refuel_command:error_fuel:object", new { Object = oil.name }]));
                    oil.askFill(oil.capacity);
                    BarricadeManager.sendFuel(raycast.transform, oil.fuel);
                    PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                        m_StringLocalizer["refuel_command:succeed:oil", new { Object = oil.name }]));
                    return;
                }
                else if (interactable is InteractableTank { source: ETankSource.FUEL } tank)
                {
                    if (!tank.isRefillable)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_fuel:object", new { Object = tank.name }]));
                    tank.ServerSetAmount(tank.capacity);
                    PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                        m_StringLocalizer["refuel_command:succeed:tank", new { Object = tank.name }]));
                    return;
                }
                else if (interactable is InteractableObjectResource { objectAsset: { interactability: EObjectInteractability.FUEL } } @object)
                {
                    if (!@object.isRefillable)
                        throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_fuel:object", new { Object = @object.name }]));
                    ObjectManager.updateObjectResource(interactable.transform, @object.capacity, true);
                    PrintAsync(string.Format("{0}{1}", (config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : ""),
                        m_StringLocalizer["refuel_command:succeed:object", new { Object = @object.name }]));
                    return;
                }
            }
            throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["refuel_command:prefix"] : "",
                     m_StringLocalizer["refuel_command:error_null"]));
        }

        private bool RefuelVehicle(InteractableVehicle vehicle)
        {
            if (!vehicle.usesFuel || vehicle.fuel >= vehicle.asset.fuel || vehicle.isExploded)
                return false;
            vehicle.fuel = vehicle.asset.fuel;
            VehicleManager.sendVehicleFuel(vehicle, vehicle.fuel);
            return true;
        }

    }
}
