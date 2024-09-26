using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;
using System.Linq;
using System.Reflection;

namespace Alpalis.AdminManager.Commands;

public sealed class RespawnCommand
{
    [Command("respawn")]
    [CommandSyntax("<zombie/animals/vehicles>")]
    [CommandDescription("Respawns entities.")]
    public sealed class Root(
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }

    }

    [Command("animals")]
    [CommandAlias("animal")]
    [CommandDescription("Respawns animals.")]
    [CommandParent(typeof(Root))]
    public sealed class Animals(IStringLocalizer stringLocalizer, IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly FieldInfo m_LastDead = typeof(Animal).GetField("_lastDead",
            BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["respawn_command:prefix"],
                     m_StringLocalizer["respawn_command:error_adminmode"]));
            int amount = 0;
            await UniTask.SwitchToMainThread();
            foreach (Animal animal in AnimalManager.animals.Where(x => x.isDead))
            {
                m_LastDead.SetValue(animal, 0f);
                amount++;
            }
            if (amount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                     m_StringLocalizer["respawn_command:animals:error_null"]));
            await PrintAsync(string.Format("{0}{1}",
                Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                m_StringLocalizer["respawn_command:animals:succeed", new { Amount = amount }]));
        }
    }

    [Command("zombies")]
    [CommandAlias("zombie")]
    [CommandDescription("Respawns zombies.")]
    [CommandParent(typeof(Root))]
    public sealed class Zombies(
        IStringLocalizer stringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly FieldInfo m_LastDead = typeof(Zombie).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo m_LastWave = typeof(ZombieManager).GetField("lastWave", BindingFlags.NonPublic | BindingFlags.Static);
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["respawn_command:prefix"],
                     m_StringLocalizer["respawn_command:error_adminmode"]));
            m_LastWave.SetValue(null, 0f);
            int amount = 0;
            await UniTask.SwitchToMainThread();
            for (int i = 0; i < LevelNavigation.bounds.Count; i++)
            {
                ZombieRegion region = ZombieManager.regions[i];
                foreach (Zombie zombie in region.zombies.Where(z => z.isDead))
                {
                    m_LastDead.SetValue(zombie, 0f);
                    amount++;
                }
            }
            if (amount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                     Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                     m_StringLocalizer["respawn_command:zombies:error_null"]));
            await PrintAsync(string.Format("{0}{1}",
                Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                m_StringLocalizer["respawn_command:zombies:succeed", new { Amount = amount }]));
        }

    }

    [Command("vehicles")]
    [CommandAlias("vehicle")]
    [CommandDescription("Respawns vehicles.")]
    [CommandParent(typeof(Root))]
    public sealed class Vehicles(
        IStringLocalizer stringLocalizer,
        IAdminSystem adminSystem,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;
        private readonly IAdminSystem m_AdminSystem = adminSystem;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 0)
                throw new CommandWrongUsageException(Context);
            if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["respawn_command:prefix"],
                     m_StringLocalizer["respawn_command:error_adminmode"]));
            await UniTask.SwitchToMainThread();
            VehicleManager.askVehicleDestroyAll();
            await PrintAsync(string.Format("{0}{1}",
                Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                m_StringLocalizer["respawn_command:vehicles:succeed"]));
        }
    }
}
