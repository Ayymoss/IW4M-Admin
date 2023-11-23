﻿using System.Text.RegularExpressions;
using Data.Models.Client;
using SharedLibraryCore;
using SharedLibraryCore.Commands;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Interfaces;

namespace IW4MAdmin.Plugins.Mute.Commands;

public class TempMuteCommand : Command
{
    private readonly MuteManager _muteManager;
    private const string TempBanRegex = @"([0-9]+\w+)\ (.+)";

    public TempMuteCommand(CommandConfiguration config, ITranslationLookup translationLookup, MuteManager muteManager,
        MuteConfiguration muteConfiguration) : base(config, translationLookup)
    {
        _muteManager = muteManager;
        Name = "tempmute";
        Description = translationLookup["PLUGINS_MUTE_COMMANDS_TEMPMUTE_DESC"];
        Alias = "tm";
        Permission = EFClient.Permission.Moderator;
        RequiresTarget = true;
        SupportedGames = muteConfiguration.GameCommands.Select(x => x.GameName).ToArray();
        Arguments = new[]
        {
            new CommandArgument
            {
                Name = translationLookup["COMMANDS_ARGS_PLAYER"],
                Required = true
            },
            new CommandArgument
            {
                Name = translationLookup["COMMANDS_ARGS_DURATION"],
                Required = true
            },
            new CommandArgument
            {
                Name = translationLookup["COMMANDS_ARGS_REASON"],
                Required = true
            }
        };
    }

    public override async Task ExecuteAsync(GameEvent gameEvent)
    {
        if (gameEvent.Origin.ClientId == gameEvent.Target.ClientId)
        {
            gameEvent.Origin.Tell(_translationLookup["COMMANDS_DENY_SELF_TARGET"]);
            return;
        }
        
        var match = Regex.Match(gameEvent.Data, TempBanRegex);
        if (match.Success)
        {
            var expiration = DateTime.UtcNow + match.Groups[1].ToString().ParseTimespan();
            var reason = match.Groups[2].ToString();

            if (await _muteManager.Mute(gameEvent.Owner, gameEvent.Origin, gameEvent.Target, expiration, reason))
            {
                gameEvent.Origin.Tell(_translationLookup["PLUGINS_MUTE_COMMANDS_TEMPMUTE_TEMPMUTED"]
                    .FormatExt(gameEvent.Target.CleanedName));
                gameEvent.Target.Tell(_translationLookup["PLUGINS_MUTE_COMMANDS_TEMPMUTE_TARGET_TEMPMUTED"]
                    .FormatExt(expiration.HumanizeForCurrentCulture(), reason));
                return;
            }

            gameEvent.Origin.Tell(_translationLookup["PLUGINS_MUTE_COMMANDS_MUTE_NOT_UNMUTED"]
                .FormatExt(gameEvent.Target.CleanedName));
            return;
        }

        gameEvent.Origin.Tell(_translationLookup["PLUGINS_MUTE_COMMANDS_TEMPMUTE_BAD_FORMAT"]);
    }
}
