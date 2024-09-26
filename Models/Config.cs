using Alpalis.UtilityServices.Models;
using System;
using System.Collections.Generic;

namespace Alpalis.AdminManager.Models;

/// <summary>
/// Configuation for AdminManager plugin.
/// </summary>
[Serializable]
public sealed class Config : MainConfig
{
    /// <summary>
    /// Is admin mode disabled?
    /// </summary>
    public bool DisableAdminMode { get; set; } = false;

    public override List<KeyValuePair<string, string>> GetPropertiesInString() =>
        [
            new ("DisableAdminMode", $"{DisableAdminMode}")
        ];
}
