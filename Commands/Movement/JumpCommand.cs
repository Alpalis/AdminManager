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

namespace Alpalis.AdminManager.Commands.Movement
{
    #region Command Parameters
    [Command("jump")]
    [CommandAlias("jmp")]
    [CommandDescription("Jump to where you're looking.")]
    [CommandActor(typeof(UnturnedUser))]
    #endregion Command Parameters
    public class JumpCommand : UnturnedCommand
    {
        #region Member Variables
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IConfigurationManager m_ConfigurationManager;
        private readonly IAdminSystem m_AdminSystem;
        private readonly Main m_Plugin;
        private readonly int COLLISION_NO_SKY = RayMasks.BLOCK_COLLISION - RayMasks.SKY;
        #endregion Member Variables

        #region Class Constructor
        public JumpCommand(
            IStringLocalizer stringLocalizer,
            IConfigurationManager configurationManager,
            IAdminSystem adminSystem,
            IPluginAccessor<Main> plugin,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_ConfigurationManager = configurationManager;
            m_AdminSystem = adminSystem;
            m_Plugin = plugin.Instance!;
        }
        #endregion Class Constructor

        protected override async UniTask OnExecuteAsync()
        {
            Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["jump_command:prefix"] : "",
                     m_StringLocalizer["jump_command:error_adminmode"]));
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            Transform aim = user.Player.Player.look.aim;
            if (!PhysicsUtility.raycast(new Ray(aim.position, aim.forward), out RaycastHit hit, 1024f, COLLISION_NO_SKY))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["jump_command:prefix"] : "",
                     m_StringLocalizer["jump_command:error_null"]));
            await user.Player.Player.TeleportToLocationUnsafeAsync(hit.point + new Vector3(0f, 2f, 0f));
            PrintAsync(string.Format("{0}{1}",
                     config.MessagePrefix ? m_StringLocalizer["jump_command:prefix"] : "",
                     m_StringLocalizer["jump_command:succeed"]));
        }
    }
}
