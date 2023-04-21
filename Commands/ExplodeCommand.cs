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

namespace Alpalis.AdminManager.Commands
{
    [Command("explode")]
    [CommandDescription("Allows to explode anything you want.")]
    [CommandSyntax("[radius] [damage]")]
    [CommandActor(typeof(UnturnedUser))]
    public class ExplodeCommand : UnturnedCommand
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly IAdminSystem m_AdminSystem;

        public ExplodeCommand(
            IStringLocalizer stringLocalizer,
            IAdminSystem adminSystem,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_AdminSystem = adminSystem;
        }

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length > 2)
                throw new CommandWrongUsageException(Context);
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["explode_command:prefix"],
                     m_StringLocalizer["explode_command:error_adminmode"]));
            float radius = 10f;
            float damage = 100f;
            if (Context.Parameters.Length >= 1 && !Context.Parameters.TryGet(0, out radius))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["explode_command:prefix"],
                     m_StringLocalizer["explode_command:error_radius"]));

            if (Context.Parameters.Length == 2 && !Context.Parameters.TryGet(1, out damage))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["explode_command:prefix"],
                     m_StringLocalizer["explode_command:error_damage"]));
            Transform aim = user.Player.Player.look.aim;
            RaycastInfo raycast = DamageTool.raycast(new(aim.position, aim.forward), 512f, RayMasks.DAMAGE_SERVER);
            if (raycast == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["explode_command:prefix"],
                     m_StringLocalizer["explode_command:error_null"]));

            await UniTask.SwitchToMainThread();
            TriggerEffectParameters effect = new(Guid.Parse("61d63a01-6a64-48ff-b6e4-432a4c6a6ee1"))
            {
                relevantDistance = radius > 240 ? EffectManager.INSANE : radius > 60 ? EffectManager.LARGE : EffectManager.MEDIUM,
                position = raycast.point,
                reliable = true
            };
            effect.SetUniformScale(radius / 10);
            EffectManager.triggerEffect(effect);
            DamageTool.explode(raycast.point, radius, EDeathCause.KILL, user.SteamId, damage, damage, damage, damage,
                damage, damage, damage, damage, out _);

            await Context.Actor.PrintMessageAsync(string.Format("{0}{1}",
                     m_StringLocalizer["explode_command:prefix"],
                     m_StringLocalizer["explode_command:succeed"]));
        }
    }
}
