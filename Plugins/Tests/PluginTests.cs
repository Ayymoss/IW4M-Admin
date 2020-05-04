﻿using IW4MAdmin.Application;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using Xunit;

namespace Tests
{
    [Collection("ManagerCollection")]
    public class PluginTests
    {
        readonly ApplicationManager Manager;

        public PluginTests(ManagerFixture fixture)
        {
            Manager = fixture.Manager;
        }

        [Fact]
        public void ClientSayObjectionalWordShouldWarn()
        {
            var e = new GameEvent()
            {
                Type = GameEvent.EventType.Connect,
                Origin = new EFClient()
                {
                    Name = $"Player1",
                    NetworkId = 1,
                    ClientNumber = 1
                },
                Owner = Manager.GetServers()[0]
            };

            Manager.AddEvent(e);
            e.Complete();

            var client = Manager.GetServers()[0].Clients[0];

            e = new GameEvent()
            {
                Type = GameEvent.EventType.Say,
                Origin = client,
                Data = "nigger",
                Owner = e.Owner
            };

            Manager.AddEvent(e);
            e.Complete();

            Assert.True(client.Warnings == 1, "client wasn't warned for objectional language");
        }
    }
}
