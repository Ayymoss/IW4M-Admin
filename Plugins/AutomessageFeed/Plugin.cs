﻿using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.SyndicationFeed.Rss;
using SharedLibraryCore.Configuration;
using System.Xml;
using Microsoft.SyndicationFeed;
using System.Collections.Generic;
using SharedLibraryCore.Helpers;
using System.Text.RegularExpressions;

namespace AutomessageFeed
{
    public class Plugin : IPlugin
    {
        public string Name => "Automessage Feed";

        public float Version => (float)Utilities.GetVersionAsDouble();

        public string Author => "RaidMax";

        private int _currentFeedItem;
        private readonly IConfigurationHandler<Configuration> _configurationHandler;

        public Plugin(IConfigurationHandlerFactory configurationHandlerFactory)
        {
            _configurationHandler = configurationHandlerFactory.GetConfigurationHandler<Configuration>("AutomessageFeedPluginSettings");
        }

        private async Task<string> GetNextFeedItem(Server server)
        {
            var items = new List<string>();

            using (var reader = XmlReader.Create(_configurationHandler.Configuration().FeedUrl, new XmlReaderSettings() { Async = true }))
            {
                var feedReader = new RssFeedReader(reader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            var item = await feedReader.ReadItem();
                            items.Add(Regex.Replace(item.Title, @"\<.+\>.*\</.+\>", ""));
                            break;
                    }
                }
            }

            if (_currentFeedItem < items.Count && (_configurationHandler.Configuration().MaxFeedItems == 0 || _currentFeedItem < _configurationHandler.Configuration().MaxFeedItems))
            {
                _currentFeedItem++;
                return items[_currentFeedItem - 1];
            }

            _currentFeedItem = 0;
            return Utilities.CurrentLocalization.LocalizationIndex["PLUGINS_AUTOMESSAGEFEED_NO_ITEMS"];
        }

        public Task OnEventAsync(GameEvent E, Server S)
        {
            return Task.CompletedTask;
        }

        public async Task OnLoadAsync(IManager manager)
        {
            await _configurationHandler.BuildAsync();
            if (_configurationHandler.Configuration() == null)
            {
                _configurationHandler.Set((Configuration)new Configuration().Generate());
                await _configurationHandler.Save();
            }

            manager.GetMessageTokens().Add(new MessageToken("FEED", GetNextFeedItem));
        }

        public Task OnTickAsync(Server S)
        {
            throw new NotImplementedException();
        }

        public Task OnUnloadAsync()
        {
            return Task.CompletedTask;
        }
    }
}
