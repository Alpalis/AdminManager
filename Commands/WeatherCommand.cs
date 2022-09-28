using Alpalis.AdminManager.API;
using Alpalis.AdminManager.Models;
using Alpalis.AdminManager.Services;
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
using System.Collections.Generic;
using System.Net;

namespace Alpalis.AdminManager.Commands
{
    public class WeatherCommand
    {
        #region Commad Parameters
        [Command("weather")]
        [CommandSyntax("<set/disable>")]
        [CommandDescription("Command to change overworld weather.")]
        #endregion Command Parameters
        public class WeatherRoot : UnturnedCommand
        {
            #region Class Constructor
            public WeatherRoot(
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
        [Command("set")]
        [CommandSyntax("<sunny/storm/blizzard>")]
        [CommandDescription("Command to set overworld weather.")]
        [CommandParent(typeof(WeatherRoot))]
        #endregion Command Parameters
        public class WeatherSet : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public WeatherSet(
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
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["weather_command:prefix"] : "",
                         m_StringLocalizer["weather_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                if (!Context.Parameters.TryGet(0, out string? weather) || weather == null ||
                    (weather != "storm" && weather != "sunny" && weather != "blizzard"))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                        m_StringLocalizer["weather_command:set:error_weather"]));
                switch(weather)
                {
                    case "sunny":
                        LightingManager.ResetScheduledWeather();
                        break;
                    case "storm":
                        LightingManager.ForecastWeatherImmediately(WeatherAssetBase.DEFAULT_RAIN.get());
                        break;
                    case "blizzard":
                        LightingManager.ForecastWeatherImmediately(WeatherAssetBase.DEFAULT_SNOW.get());
                        break;
                }
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                    m_StringLocalizer["weather_command:set:succeed", new
                    {
                        Weather = m_StringLocalizer[string.Format("weather_command:weathers:{0}", weather)]
                    }]));
            }
        }

        #region Commad Parameters
        [Command("disable")]
        [CommandDescription("Command to disable overworld weather.")]
        [CommandParent(typeof(WeatherRoot))]
        #endregion Command Parameters
        public class WeatherDisable : UnturnedCommand
        {
            #region Member Variables
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IConfigurationManager m_ConfigurationManager;
            private readonly IAdminSystem m_AdminSystem;
            private readonly Main m_Plugin;
            #endregion Member Variables

            #region Class Constructor
            public WeatherDisable(
                IStringLocalizer StringLocalizer,
                IConfigurationManager configurationManager,
                IAdminSystem adminSystem,
                IPluginAccessor<Main> plugin,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = StringLocalizer;
                m_ConfigurationManager = configurationManager;
                m_AdminSystem = adminSystem;
                m_Plugin = plugin.Instance!;
            }
            #endregion Class Constructor

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                Config config = m_ConfigurationManager.GetConfig<Config>(m_Plugin);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         config.MessagePrefix ? m_StringLocalizer["weather_command:prefix"] : "",
                         m_StringLocalizer["weather_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                LightingManager.DisableWeather();
                PrintAsync(string.Format("{0}{1}", config.MessagePrefix && Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                    m_StringLocalizer["weather_command:disable"]));
            }
        }
    }
}
