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
    #region Command Parameters
    [Command("more")]
    [CommandSyntax("[amount]")]
    [CommandDescription("Command to duplicate the item you are holding.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class MoreCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public MoreCommand(
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
            if (Context.Parameters.Count != 0 && Context.Parameters.Count != 1)
                throw new CommandWrongUsageException(Context);
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["more_command:prefix"] : "",
                     m_StringLocalizer["more_command:error_adminmode"]));
            PlayerEquipment equipment = user.Player.Player.equipment;
            if (equipment.itemID == 0)
                throw new UserFriendlyException(m_StringLocalizer["more_command:error_null"]);
            Item item = new(equipment.itemID, EItemOrigin.ADMIN);
            await UniTask.SwitchToMainThread();
            if (Context.Parameters.Count == 0)
            {
                user.Player.Player.inventory.forceAddItem(item, true);
                PrintAsync(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["more_command:prefix"] : "",
                     m_StringLocalizer["more_command:succeed:one",
                     new { ItemName = equipment.asset.itemName, ItemID = equipment.asset.id }]));
                return;
            }
            if (!Context.Parameters.TryGet(0, out ushort itemAmount) || itemAmount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                config.MessagePrefix ? m_StringLocalizer["more_command:prefix"] : "",
                m_StringLocalizer["more_command:error_null_number"]));
            if (itemAmount > 100)
                throw new UserFriendlyException(string.Format("{0}{1}",
                config.MessagePrefix ? m_StringLocalizer["more_command:prefix"] : "",
                m_StringLocalizer["more_command:error_high_number"]));
            for (int i = 0; i < itemAmount; i++)
                user.Player.Player.inventory.forceAddItem(item, true);
            PrintAsync(string.Format("{0}{1}",
                 config.MessagePrefix ? m_StringLocalizer["more_command:prefix"] : "",
                 m_StringLocalizer["more_command:succeed:many",
                 new { ItemName = equipment.asset.itemName, ItemID = equipment.asset.id, ItemAmount = itemAmount }]));
        }
    }
}
