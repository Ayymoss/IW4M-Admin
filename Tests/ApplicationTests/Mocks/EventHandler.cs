﻿using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System.Collections.Generic;

namespace ApplicationTests.Mocks
{
    class MockEventHandler : IEventHandler
    {
        public IList<GameEvent> Events = new List<GameEvent>();
        private readonly bool _autoExecute;

        public MockEventHandler(bool autoExecute = false)
        {
            _autoExecute = autoExecute;
        }

        public void AddEvent(GameEvent gameEvent)
        {
            Events.Add(gameEvent);

            if (_autoExecute)
            {
                gameEvent.Owner?.ExecuteEvent(gameEvent);
                gameEvent.Complete();
            }
        }
    }
}
