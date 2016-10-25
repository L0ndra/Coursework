using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Coursework.Data;
using Coursework.Data.Entities;
using Coursework.Data.IONetwork;
using Coursework.Data.MessageServices;
using Coursework.Data.Util;
using Coursework.Gui.Initializers;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkInfoRetrieverTests
    {
        private INetworkInfoRetriever _networkInfoRetriever;
        private INetworkHandler _network;
        private static string WriteFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "test.json");
        private static string ReadFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "testnetworkfile.json");

        [SetUp]
        public void Setup()
        {
            Mapper.Initialize(MapperInitializer.InitializeMapper);

            _networkInfoRetriever = new NetworkInfoRetriever();

            _network = new Network();

            var nodes = new[]
            {
                new Node
                {
                    Id = 0,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {1}),
                    MessageQueue = new List<MessageQueueHandler>()
                },
                new Node
                {
                    Id = 1,
                    LinkedNodesId = new SortedSet<uint>(new uint[] {0}),
                    MessageQueue = new List<MessageQueueHandler>()
                }
            };

            foreach (var node in nodes)
            {
                _network.AddNode(node);
            }

            var channels = new[]
            {
                new Channel
                {
                    Id = Guid.Empty,
                    ConnectionType = ConnectionType.Duplex,
                    ChannelType = ChannelType.Ground,
                    ErrorChance = 0.5,
                    SecondNodeId = 0,
                    FirstNodeId = 1,
                    Price = 20
                }
            };

            foreach (var channel in channels)
            {
                _network.AddChannel(channel);
            }
        }

        [Test]
        public void ReadShouldReturnCorrectNetworkFromFile()
        {
            // Arrange
            // Act
            var network = _networkInfoRetriever.Read(ReadFilename);

            // Assert
            Assert.That(network.Nodes.Length, Is.EqualTo(_network.Nodes.Length));
            Assert.That(network.Channels.Length, Is.EqualTo(_network.Channels.Length));

            foreach (var node in network.Nodes)
            {
                Assert.IsTrue(_network.Nodes.Any(n => n.Id == node.Id));
            }

            foreach (var channel in network.Channels)
            {
                var currentChannel = _network.Channels
                    .First(c => c.Id == channel.Id);

                Assert.That(channel.FirstNodeId, Is.EqualTo(currentChannel.FirstNodeId));
                Assert.That(channel.SecondNodeId, Is.EqualTo(currentChannel.SecondNodeId));
                Assert.That(channel.ChannelType, Is.EqualTo(currentChannel.ChannelType));
                Assert.That(channel.ConnectionType, Is.EqualTo(currentChannel.ConnectionType));
                Assert.That(channel.Price, Is.EqualTo(currentChannel.Price));
                Assert.That(channel.ErrorChance, Is.EqualTo(currentChannel.ErrorChance));
            }
        }

        [Test]
        public void WriteShouldWriteNetworkToFileWithoutErrors()
        {
            // Arrange
            // Act
            _networkInfoRetriever.Write(WriteFilename, _network);

            // Assert
            using (var testFile = new StreamReader(ReadFilename))
            using (var writedFile = new StreamReader(WriteFilename))
            {
                while (!testFile.EndOfStream)
                {
                    var testString = testFile.ReadLine();
                    var writedString = writedFile.ReadLine();

                    Assert.That(testString, Is.EqualTo(writedString));
                }

                Assert.IsTrue(writedFile.EndOfStream);
            }
        }
    }
}
