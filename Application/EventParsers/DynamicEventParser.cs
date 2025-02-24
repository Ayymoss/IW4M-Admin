﻿using SharedLibraryCore.Configuration;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Interfaces.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace IW4MAdmin.Application.EventParsers
{
    /// <summary>
    /// empty generic implementation of the IEventParserConfiguration
    /// allows script plugins to generate dynamic event parsers
    /// </summary>
    sealed internal class DynamicEventParser : BaseEventParser
    {
        public DynamicEventParser(IParserRegexFactory parserRegexFactory, ILogger logger,
            ApplicationConfiguration appConfig, IGameScriptEventFactory gameScriptEventFactory) : base(
            parserRegexFactory, logger, appConfig, gameScriptEventFactory)
        {
        }
    }
}
