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
using System.Linq;
using System.Reflection;

namespace Alpalis.AdminManager.Commands
{
    public class RespawnCommand
    {
        #region Commad Parameters
        [Command("respawn")]
        [CommandSyntax("<zombie/animals/vehicles>")]
        [CommandDescription("Command to respawn entities.")]
        #endregion Command Parameters
        public class RespawnRoot : UnturnedCommand
        {
            #region Class Constructor
            public RespawnRoot(
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                throw new CommandWrongUsageException(Context);
            }

        }

        #region Commad Parameters
        [Command("animals")]
        [CommandAlias("animal")]
        [CommandDescription("Command to respawn animals.")]
        [CommandParent(typeof(RespawnRoot))]
        #endregion Command Parameters
        public class Animals : UnturnedCommand
        {
            #region Member Variables
            private readonly FieldInfo m_LastDead;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Animals(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_LastDead = typeof(Animal).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["respawn_command:prefix"] : "",
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
                         config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                         m_StringLocalizer["respawn_command:animals:error_null"]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:animals:succeed", new { Amount = amount }]));
            }
        }

        #region Commad Parameters
        [Command("zombies")]
        [CommandAlias("zombie")]
        [CommandDescription("Command to respawn zombies.")]
        [CommandParent(typeof(RespawnRoot))]
        #endregion Command Parameters
        public class Zombies : UnturnedCommand
        {
            #region Member Variables
            private readonly FieldInfo m_LastDead;
            private readonly FieldInfo m_LastWave;
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Zombies(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_LastDead = typeof(Zombie).GetField("_lastDead", BindingFlags.NonPublic | BindingFlags.Instance);
                m_LastWave = typeof(ZombieManager).GetField("lastWave", BindingFlags.NonPublic | BindingFlags.Static);
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["respawn_command:prefix"] : "",
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
                         config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                         m_StringLocalizer["respawn_command:zombies:error_null"]));
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:zombies:succeed", new { Amount = amount }]));
            }

        }

        #region Commad Parameters
        [Command("vehicles")]
        [CommandAlias("vehicle")]
        [CommandDescription("Command to respawn vehicles.")]
        [CommandParent(typeof(RespawnRoot))]
        #endregion Command Parameters
        public class Vehicles : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public Vehicles(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IConfigurationManager configurationManager,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
                m_ConfigurationManager = configurationManager;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Length != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (Context.Actor is UnturnedUser user && !m_AdminSystem.IsInAdminMode(user))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["respawn_command:prefix"] : "",
                         m_StringLocalizer["respawn_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                VehicleManager.askVehicleDestroyAll();
                PrintAsync(string.Format("{0}{1}",
                    config.MessagePrefix && Context.Actor is UnturnedUser ? m_StringLocalizer["respawn_command:prefix"] : "",
                    m_StringLocalizer["respawn_command:vehicles:succeed"]));
            }
        }
    }
}
