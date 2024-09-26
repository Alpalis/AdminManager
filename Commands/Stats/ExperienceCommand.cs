using Alpalis.AdminManager.API;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OpenMod.API.Commands;
using OpenMod.API.Permissions;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Permissions;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using Steamworks;
using System;

namespace Alpalis.AdminManager.Commands;

public sealed class ExperienceCommand
{
    [Command("experience")]
    [CommandAlias("exp")]
    [CommandDescription("Manages your or somebody's experience.")]
    [CommandSyntax("<get/add/set/take>")]
    public sealed class Root(
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        protected override UniTask OnExecuteAsync()
        {
            throw new CommandWrongUsageException(Context);
        }
    }

    [Command("add")]
    [CommandSyntax("<amount> [player]")]
    [CommandDescription("Adds your or somebody's experience.")]
    [RegisterCommandPermission("other", Description = "Allows to add experience of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class AddUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:error_adminmode"]));
            if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:add:error_amount"]));
            if (Context.Parameters.Count == 1)
            {
                await UniTask.SwitchToMainThread();
                if (user.Player.Player.skills.experience + amount > int.MaxValue)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:add:error_amount"]));
                user.Player.Player.skills.ServerModifyExperience((int)amount);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:add:succeed:yourself", new { Amount = amount, NewExperience = user.Player.Player.skills.experience }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            if (targetSPlayer.player.skills.experience + amount > int.MaxValue)
                throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["experience_command:prefix"],
                 m_StringLocalizer["experience_command:add:error_amount"]));
            targetSPlayer.player.skills.ServerModifyExperience((int)amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:add:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:add:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
        }
    }

    [Command("add")]
    [CommandSyntax("<amount> <player>")]
    [CommandDescription("Adds somebody's experience.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class AddConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:add:error_amount"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:error_player"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            if (targetSPlayer.player.skills.experience + amount > int.MaxValue)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:add:error_amount"]);
            targetSPlayer.player.skills.ServerModifyExperience((int)amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:add:succeed:somebody:console", new
                {
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(m_StringLocalizer["experience_command:add:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                Amount = amount,
                NewExperience = targetSPlayer.player.skills.experience
            }]);
        }
    }

    [Command("set")]
    [CommandSyntax("<amount> [player]")]
    [CommandDescription("Sets your or somebody's experience.")]
    [RegisterCommandPermission("other", Description = "Allows to set experience of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class SetUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:error_adminmode"]));
            if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount))
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:set:error_amount"]));
            if (Context.Parameters.Count == 1)
            {
                await UniTask.SwitchToMainThread();
                user.Player.Player.skills.ServerSetExperience(amount);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:set:succeed:yourself", new { Experience = user.Player.Player.skills.experience }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetSPlayer.player.skills.ServerSetExperience(amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:set:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Experience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:set:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Experience = targetSPlayer.player.skills.experience
                }]));
        }
    }

    [Command("set")]
    [CommandSyntax("<amount> <player>")]
    [CommandDescription("Sest your or somebody's experience.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class SetConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount))
                throw new UserFriendlyException(m_StringLocalizer["experience_command:set:error_amount"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:error_player"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            targetSPlayer.player.skills.ServerSetExperience(amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:set:succeed:somebody:console", new
                {
                    Experience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(m_StringLocalizer["experience_command:set:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                Experience = targetSPlayer.player.skills.experience
            }]);
        }
    }

    [Command("take")]
    [CommandSyntax("<amount> [player]")]
    [CommandDescription("Takes your or somebody's experience.")]
    [RegisterCommandPermission("other", Description = "Allows to take experience of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class TakeUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:error_adminmode"]));
            if (Context.Parameters.Count != 1 && Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:take:error_amount"]));
            if (Context.Parameters.Count == 1)
            {
                await UniTask.SwitchToMainThread();
                if ((int)user.Player.Player.skills.experience - amount < 0)
                    throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:take:error_amount"]));
                user.Player.Player.skills.ServerModifyExperience(-(int)amount);
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:take:succeed:yourself", new { Amount = amount, NewExperience = user.Player.Player.skills.experience }]));
                return;
            }
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:error_player"]));
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            if ((int)targetSPlayer.player.skills.experience - amount < 0)
                throw new UserFriendlyException(string.Format("{0}{1}",
                 m_StringLocalizer["experience_command:prefix"],
                 m_StringLocalizer["experience_command:take:error_amount"]));
            targetSPlayer.player.skills.ServerModifyExperience(-(int)amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:take:succeed:somebody:player", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:take:succeed:somebody:executor", new
                {
                    PlayerName = targetSPlayer.playerID.playerName,
                    CharacterName = targetSPlayer.playerID.characterName,
                    NickName = targetSPlayer.playerID.nickName,
                    SteamID = targetSteamID,
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
        }
    }

    [Command("take")]
    [CommandSyntax("<amount> <player>")]
    [CommandDescription("Takes somebody's experience.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class TakeConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Count != 2)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out uint amount) || amount == 0)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:take:error_amount"]);
            if (!Context.Parameters.TryGet(1, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:error_player"]);
            SteamPlayer targetSPlayer = targetUser.Player.SteamPlayer;
            CSteamID targetSteamID = targetSPlayer.playerID.steamID;
            await UniTask.SwitchToMainThread();
            if ((int)targetSPlayer.player.skills.experience - amount < 0)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:take:error_amount"]);
            targetSPlayer.player.skills.ServerModifyExperience(-(int)amount);
            await targetUser.PrintMessageAsync(string.Format("{0}{1}",
                m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:take:succeed:somebody:console", new
                {
                    Amount = amount,
                    NewExperience = targetSPlayer.player.skills.experience
                }]));
            await PrintAsync(m_StringLocalizer["experience_command:take:succeed:somebody:executor", new
            {
                PlayerName = targetSPlayer.playerID.playerName,
                CharacterName = targetSPlayer.playerID.characterName,
                NickName = targetSPlayer.playerID.nickName,
                SteamID = targetSteamID,
                Amount = amount,
                NewExperience = targetSPlayer.player.skills.experience
            }]);
        }
    }

    [Command("get")]
    [CommandSyntax("[player]")]
    [CommandDescription("Gets your or somebody's experience.")]
    [RegisterCommandPermission("other", Description = "Allows to get experience of other player.")]
    [CommandActor(typeof(UnturnedUser))]
    [CommandParent(typeof(Root))]
    public sealed class GetUnturned(
        IAdminSystem adminSystem,
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IAdminSystem m_AdminSystem = adminSystem;
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            if (Context.Parameters.Length == 0)
            {
                await PrintAsync(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:get:yourself", new { Experience = user.Player.Player.skills.experience }]));
                return;
            }
            else if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (await CheckPermissionAsync("other") != PermissionGrantResult.Grant)
                throw new NotEnoughPermissionException(Context, "other");
            if (!m_AdminSystem.IsInAdminMode(user))
                throw new UserFriendlyException(string.Format("{0}{1}",
                     m_StringLocalizer["experience_command:prefix"],
                     m_StringLocalizer["experience_command:error_adminmode"]));
            if (!Context.Parameters.TryGet(0, out UnturnedUser? targetUser) || targetUser == null)
                throw new UserFriendlyException(string.Format("{0}{1}",
                    m_StringLocalizer["experience_command:prefix"],
                    m_StringLocalizer["experience_command:error_player"]));
            SteamPlayer sPlayer = targetUser.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await PrintAsync(string.Format("{0}{1}", m_StringLocalizer["experience_command:prefix"],
                m_StringLocalizer["experience_command:get:somebody", new
                {
                    PlayerName = sPlayer.playerID.playerName,
                    CharacterName = sPlayer.playerID.characterName,
                    NickName = sPlayer.playerID.nickName,
                    SteamID = steamID,
                    Experience = sPlayer.player.skills.experience
                }]));
        }
    }

    [Command("get")]
    [CommandSyntax("<player>")]
    [CommandDescription("Gets somebody's experience.")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandParent(typeof(Root))]
    public sealed class GetConsole(
        IStringLocalizer stringLocalizer,
        IServiceProvider serviceProvider) : UnturnedCommand(serviceProvider)
    {
        private readonly IStringLocalizer m_StringLocalizer = stringLocalizer;

        protected override async UniTask OnExecuteAsync()
        {
            if (Context.Parameters.Length != 1)
                throw new CommandWrongUsageException(Context);
            if (!Context.Parameters.TryGet(0, out UnturnedUser? user) || user == null)
                throw new UserFriendlyException(m_StringLocalizer["experience_command:error_player"]);
            SteamPlayer sPlayer = user.Player.SteamPlayer;
            CSteamID steamID = sPlayer.playerID.steamID;
            await PrintAsync(m_StringLocalizer["experience_command:get:somebody", new
            {
                PlayerName = sPlayer.playerID.playerName,
                CharacterName = sPlayer.playerID.characterName,
                NickName = sPlayer.playerID.nickName,
                SteamID = steamID,
                Experience = sPlayer.player.skills.experience
            }]);
        }
    }
}
