using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Alpalis.AdminManager.Commands.Movement
{
    [Command("jump")]
    [CommandAlias("jmp")]
    [CommandDescription("Jump to where you're looking.")]
    [CommandActor(typeof(UnturnedUser))]
    public class JumpCommand : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IAdminSystem m_AdminSystem;
        private readonly int COLLISION_NO_SKY = RayMasks.BLOCK_COLLISION - RayMasks.SKY;

        public JumpCommand(
            IStringLocalizer stringLocalizer,
            IAdminSystem adminSystem,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_AdminSystem = adminSystem;
        }

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["jump_command:prefix"],
                     m_StringLocalizer["jump_command:error_adminmode"]));
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            Transform aim = user.Player.Player.look.aim;
            RaycastInfo raycast = DamageTool.raycast(new(aim.position, aim.forward), 1024f, COLLISION_NO_SKY);
            if (raycast == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["jump_command:prefix"],
                     m_StringLocalizer["jump_command:error_null"]));
            await user.Player.Player.TeleportToLocationUnsafeAsync(raycast.point + new Vector3(0f, 2f, 0f));
            PrintAsync(string.Format("{0}{1}",
                     m_StringLocalizer["jump_command:prefix"],
                     m_StringLocalizer["jump_command:succeed"]));
        }
    }
}
