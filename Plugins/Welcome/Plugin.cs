﻿using System;
using System.Threading.Tasks;

using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Services;
using SharedLibraryCore.Database.Models;
using System.Linq;
using SharedLibraryCore.Database;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using static SharedLibraryCore.Database.Models.EFClient;

namespace IW4MAdmin.Plugins.Welcome
{
    public class Plugin : IPlugin
    {
        String TimesConnected(EFClient P)
        {
            int connection = P.Connections;
            String Prefix = String.Empty;
            if (connection % 10 > 3 || connection % 10 == 0 || (connection % 100 > 9 && connection % 100 < 19))
                Prefix = "th";
            else
            {
                switch (connection % 10)
                {
                    case 1:
                        Prefix = "st";
                        break;
                    case 2:
                        Prefix = "nd";
                        break;
                    case 3:
                        Prefix = "rd";
                        break;
                }
            }

            switch (connection)
            {
                case 0:
                case 1:
                    return "first";
                case 2:
                    return "second";
                case 3:
                    return "third";
                case 4:
                    return "fourth";
                case 5:
                    return "fifth";
                default:
                    return connection.ToString() + Prefix;
            }
        }

        public string Author => "RaidMax";

        public float Version => 1.0f;

        public string Name => "Welcome Plugin";

        private readonly IConfigurationHandler<WelcomeConfiguration> _configHandler;
        private readonly IDatabaseContextFactory _contextFactory;

        public Plugin(IConfigurationHandlerFactory configurationHandlerFactory, IDatabaseContextFactory contextFactory)
        {
            _configHandler = configurationHandlerFactory.GetConfigurationHandler<WelcomeConfiguration>("WelcomePluginSettings");
            _contextFactory = contextFactory;
        }

        public async Task OnLoadAsync(IManager manager)
        {
            if (_configHandler.Configuration() == null)
            {
                _configHandler.Set((WelcomeConfiguration)new WelcomeConfiguration().Generate());
                await _configHandler.Save();
            }
        }

        public Task OnUnloadAsync() => Task.CompletedTask;

        public Task OnTickAsync(Server S) => Task.CompletedTask;

        public async Task OnEventAsync(GameEvent E, Server S)
        {
            if (E.Type == GameEvent.EventType.Join)
            {
                EFClient newPlayer = E.Origin;
                if (newPlayer.Level >= Permission.Trusted && !E.Origin.Masked)
                    E.Owner.Broadcast(await ProcessAnnouncement(_configHandler.Configuration().PrivilegedAnnouncementMessage, newPlayer));

                newPlayer.Tell(await ProcessAnnouncement(_configHandler.Configuration().UserWelcomeMessage, newPlayer));

                if (newPlayer.Level == Permission.Flagged)
                {
                    string penaltyReason;

                    await using var context = _contextFactory.CreateContext(false);
                    {
                        penaltyReason = await context.Penalties
                            .Where(p => p.OffenderId == newPlayer.ClientId && p.Type == EFPenalty.PenaltyType.Flag)
                            .OrderByDescending(p => p.When)
                            .Select(p => p.AutomatedOffense ?? p.Offense)
                            .FirstOrDefaultAsync();
                    }

                    E.Owner.ToAdmins($"^1NOTICE: ^7Flagged player ^5{newPlayer.Name} ^7({penaltyReason}) has joined!");
                }
                else
                    E.Owner.Broadcast(await ProcessAnnouncement(_configHandler.Configuration().UserAnnouncementMessage, newPlayer));
            }
        }

        private async Task<string> ProcessAnnouncement(string msg, EFClient joining)
        {
            msg = msg.Replace("{{ClientName}}", joining.Name);
            msg = msg.Replace("{{ClientLevel}}", Utilities.ConvertLevelToColor(joining.Level, joining.ClientPermission.Name));
            // this prevents it from trying to evaluate it every message
            if (msg.Contains("{{ClientLocation}}"))
            {
                msg = msg.Replace("{{ClientLocation}}", await GetCountryName(joining.IPAddressString));
            }
            msg = msg.Replace("{{TimesConnected}}", TimesConnected(joining));

            return msg;
        }

        /// <summary>
        /// makes a webrequest to determine IP origin 
        /// </summary>
        /// <param name="ip">IP address to get location of</param>
        /// <returns></returns>
        private async Task<string> GetCountryName(string ip)
        {
            using (var wc = new WebClient())
            {
                try
                {
                    string response = await wc.DownloadStringTaskAsync(new Uri($"http://extreme-ip-lookup.com/json/{ip}"));
                    var responseObj  = JObject.Parse(response);
                    response = responseObj["country"].ToString();

                    return string.IsNullOrEmpty(response) ? Utilities.CurrentLocalization.LocalizationIndex["PLUGINS_WELCOME_UNKNOWN_COUNTRY"] : response;
                }

                catch
                {
                    return Utilities.CurrentLocalization.LocalizationIndex["PLUGINS_WELCOME_UNKNOWN_IP"];
                }
            }
        }
    }
}
