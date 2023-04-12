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

namespace Alpalis.AdminManager.Commands
{
    public class RespawnCommand
    {
        [Command("respawn")]
        [CommandSyntax("<zombie/animals/vehicles>")]
        [CommandDescription("Respawns entities.")]
        public class Root : UnturnedCommand
        {
            public Root(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        [Command("animals")]
        [CommandAlias("animal")]
        [CommandDescription("Respawns animals.")]
        [CommandParent(typeof(Root))]
        public class Animals : UnturnedCommand
        {
            private readonly FieldInfo m_LastDead;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Animals(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_LastDead = typeof(Animal).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

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
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:animals:succeed", new { Amount = amount }]));
            }
        }

        [Command("zombies")]
        [CommandAlias("zombie")]
        [CommandDescription("Respawns zombies.")]
        [CommandParent(typeof(Root))]
        public class Zombies : UnturnedCommand
        {
            private readonly FieldInfo m_LastDead;
            private readonly FieldInfo m_LastWave;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Zombies(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_LastDead = typeof(Zombie).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
                m_LastWave = typeof(ZombieManager).GetField("lastWave", BindingFlags.NonPublic | BindingFlags.Static);
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

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
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:zombies:succeed", new { Amount = amount }]));
            }

        }

        [Command("vehicles")]
        [CommandAlias("vehicle")]
        [CommandDescription("Respawns vehicles.")]
        [CommandParent(typeof(Root))]
        public class Vehicles : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Vehicles(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

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
                PrintAsync(string.Format("{0}{1}",
                    Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:vehicles:succeed"]));
            }
        }
    }
}
