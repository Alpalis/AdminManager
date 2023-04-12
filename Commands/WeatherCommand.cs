using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using System;

namespace Alpalis.AdminManager.Commands
{
    public class WeatherCommand
    {
        [Command("weather")]
        [CommandSyntax("<set/disable>")]
        [CommandDescription("Manages overworld weather.")]
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

        [Command("set")]
        [CommandSyntax("<sunny/storm/blizzard>")]
        [CommandDescription("Sets overworld weather.")]
        [CommandParent(typeof(Root))]
        public class Set : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Set(
                IStringLocalizer stringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = stringLocalizer;
                m_AdminSystem = adminSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 1)
                    throw new CommandWrongUsageException(Context);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["weather_command:prefix"],
                         m_StringLocalizer["weather_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                if (!Context.Parameters.TryGet(0, out string? weather) || weather == null ||
                    (weather != "storm" && weather != "sunny" && weather != "blizzard"))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                        Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                        m_StringLocalizer["weather_command:set:error_weather"]));
                switch (weather)
                {
                    case "sunny":
                        LightingManager.ResetScheduledWeather();
                        break;
                    case "storm":
                        LightingManager.ForecastWeatherImmediately(WeatherAssetBase.DEFAULT_RAIN.Find());
                        break;
                    case "blizzard":
                        LightingManager.ForecastWeatherImmediately(WeatherAssetBase.DEFAULT_SNOW.Find());
                        break;
                }
                PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                    m_StringLocalizer["weather_command:set:succeed", new
                    {
                        Weather = m_StringLocalizer[string.Format("weather_command:weathers:{0}", weather)]
                    }]));
            }
        }

        [Command("disable")]
        [CommandDescription("Disables overworld weather.")]
        [CommandParent(typeof(Root))]
        public class Disable : UnturnedCommand
        {
            private readonly IStringLocalizer m_StringLocalizer;
            private readonly IAdminSystem m_AdminSystem;

            public Disable(
                IStringLocalizer StringLocalizer,
                IAdminSystem adminSystem,
                IServiceProvider serviceProvider) : base(serviceProvider)
            {
                m_StringLocalizer = StringLocalizer;
                m_AdminSystem = adminSystem;
            }

            protected override async UniTask OnExecuteAsync()
            {
                if (Context.Parameters.Count != 0)
                    throw new CommandWrongUsageException(Context);
                if (!m_AdminSystem.IsInAdminMode(Context.Actor))
                    throw new UserFriendlyException(string.Format("{0}{1}",
                         m_StringLocalizer["weather_command:prefix"],
                         m_StringLocalizer["weather_command:error_adminmode"]));
                await UniTask.SwitchToMainThread();
                LightingManager.DisableWeather();
                PrintAsync(string.Format("{0}{1}", Context.Actor.GetType() == typeof(UnturnedUser) ? m_StringLocalizer["weather_command:prefix"] : "",
                    m_StringLocalizer["weather_command:disable"]));
            }
        }
    }
}
