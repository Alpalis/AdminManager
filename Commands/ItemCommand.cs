/*using Alpalis.AdminManager.API;
using Alpalis.UtilityServices.API;
using Alpalis.UtilityServices.Helpers;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;
using Alpalis.AdminManager.Models;

namespace Alpalis.AdminManager.Commands
{
    #region Command Parameters
    [Command("przedmiot")]
    [CommandAlias("pr")]
    [CommandSyntax("<item ID> [amount] [player's steamID/ID]")]
    [CommandDescription("Command to give items.")]
    #endregion Command Parameters
    public class ItemCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IAdminSystem m_AdminSystem;
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IIdentityManagerImplementation m_IdentityManagerImplementation;
        private readonly Main m_Plugin;
        #endregion Member Variables

        #region Class Constructor
        public ItemCommand(
            IAdminSystem adminSystem,
            IStringLocalizer stringLocalizer,
            IConfigurationManager configurationManager,
            IIdentityManagerImplementation identityManagerImplementation,
            Main plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_AdminSystem = adminSystem;
            m_StringLocalizer = stringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_IdentityManagerImplementation = identityManagerImplementation;
            m_Plugin = plugin;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {

            if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_adminmode"]);
            if (Context.Parameters.Length < 1 || Context.Parameters.Length > 3)
                throw new CommandWrongUsageException(Context);
            if ((Context.Parameters.Length == 1 || Context.Parameters.Length == 2) && Context.Actor.GetType() == typeof(ConsoleActor))
                throw new CommandWrongActorException(Context);
            if (!Context.Parameters.TryGet(0, out string? itemName) || itemName == null ||
                !UnturnedAssetHelper.GetItem(itemName, out ItemAsset itemAsset))
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_null_item"]);
            Item item = new(itemAsset.id, EItemOrigin.ADMIN);
            await UniTask.SwitchToMainThread();
            if (Context.Parameters.Length == 1)
            {
                UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
                uPlayer.Player.Player.inventory.forceAddItem(item, true);
                PrintAsync(m_StringLocalizer["item_command:yourself_succeed",
                    new { ItemName = itemAsset.itemName, ItemID = itemAsset.id }]);
                return;
            }
            if (!Context.Parameters.TryGet(1, out ushort itemAmount))
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_null_number"]);
            if (itemAmount > 100)
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_high_number"]);
            if (Context.Parameters.Length == 2)
            {
                UnturnedUser uPlayer = (UnturnedUser)Context.Actor;
                for (int i = 0; i < itemAmount; i++)
                {
                    uPlayer.Player.Player.inventory.forceAddItem(item, true);
                }
                PrintAsync(m_StringLocalizer["item_command:yourself_many_succeed",
                    new { ItemName = itemAsset.itemName, ItemID = itemAsset.id, ItemAmount = itemAmount }]);
                return;
            }
            ulong? steamID = null;
            if (m_ConfigurationManager.GetConfig<Config>(m_Plugin).IdentityManagerImplementation)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().LastOrDefault(x => x.GetName().Name.Contains("Alpalis.IdentityManager"));
                if (assembly != null)
                {
                    if (!Context.Parameters.TryGet(2, out ushort identityID))
                        throw new UserFriendlyException(m_StringLocalizer["admin_translations:item_command:error_id"]);
                    ulong? steamid = await m_IdentityManagerImplementation.GetUlongSteamID(identityID);
                    return;
                }
            }
            await UniTask.SwitchToMainThread();
            if (steamID == null || !PlayerTool.tryGetSteamPlayer(steamID.ToString(), out SteamPlayer steamFindPlayer))
                throw new UserFriendlyException(m_StringLocalizer["item_command:error_id"]);
            for (int i = 0; i < itemAmount; i++)
            {
                steamFindPlayer.player.inventory.forceAddItem(item, true);
            }
            PrintAsync(m_StringLocalizer["item_command:somebody_many_succeed",
                new
                {
                    ItemName = itemAsset.itemName,
                    ItemID = itemAsset.id,
                    ItemAmount = itemAmount,
                    ID = steamFindPlayer.playerID.characterName
                }]);
        }
    }
}
*/