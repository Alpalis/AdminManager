﻿using Alpalis.AdminManager.Models;
using Alpalis.UtilityServices.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

#region NuGet Assembly Data
[assembly:
    PluginMetadata("Alpalis.AdminManager", Author = "Pandetthe",
        DisplayName = "Alpalis AdminManager Plugin",
        Website = "https://alpalis.eu")]
#endregion Nuget Assembly Data

namespace Alpalis.AdminManager
{
    public class Main : OpenModUnturnedPlugin
    {
        #region Member Variables
        private readonly ILogger<Main> m_Logger;
        private readonly IConfigurationManager m_ConfigurationManager;
        #endregion Member Variables

        #region Class Constructor
        public Main(
            ILogger<Main> logger,
            IConfigurationManager configurationManager,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_ConfigurationManager = configurationManager;
        }
        #endregion Class Constructor

        protected override async UniTask OnLoadAsync()
        {
            // Configuration load
            await m_ConfigurationManager.LoadConfig(this, new Config());

            // Plugin load logging
            m_Logger.LogInformation("Plugin started successfully!");
        }

        protected override async UniTask OnUnloadAsync()
        {
            // Plugin unload logging
            m_Logger.LogInformation("Plugin disabled successfully!");
        }
    }
}