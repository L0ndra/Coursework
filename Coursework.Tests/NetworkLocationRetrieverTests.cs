using System;
using System.IO;
using System.Linq;
using Coursework.Data.Constants;
using Coursework.Data.IONetwork;
using Coursework.Data.Util;
using NUnit.Framework;

namespace Coursework.Tests
{
    [TestFixture]
    public class NetworkLocationRetrieverTests
    {
        private INetworkLocationMapRetriever _networkLocationMapRetriever;
        private NodeLocationMapDto[] _nodeLocationsMap;
        private static string WriteFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "testlocation.json");
        private static string ReadFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "testnetworkfilelocation.json");

        [SetUp]
        public void Setup()
        {
            _networkLocationMapRetriever = new NetworkLocationMapRetriever();

            _nodeLocationsMap = new[]
            {
                new NodeLocationMapDto
                {
                    Id = 0,
                    X = 5.0,
                    Y = 6.0
                },
                new NodeLocationMapDto
                {
                    Id = 1,
                    X = 26.9,
                    Y = 11.1
                }
            };
        }

        [Test]
        public void ReadShouldReturnCorrectArray()
        {
            // Arrange
            // Act
            var result = _networkLocationMapRetriever.Read(ReadFilename);

            // Assert
            Assert.That(result.Length, Is.EqualTo(_nodeLocationsMap.Length));

            foreach (var resultNodeLocationDto in result)
            {
                var nodeLocationDto = _nodeLocationsMap.First(n => n.Id == resultNodeLocationDto.Id);

                Assert.That(Math.Abs(resultNodeLocationDto.X - nodeLocationDto.X), 
                    Is.LessThanOrEqualTo(AllConstants.Eps));

                Assert.That(Math.Abs(resultNodeLocationDto.Y - nodeLocationDto.Y),
                    Is.LessThanOrEqualTo(AllConstants.Eps));
            }
        }

        [Test]
        public void WriteShouldCorrectlyWriteInfoInFile()
        {
            // Arrange
            // Act
            _networkLocationMapRetriever.Write(WriteFilename, _nodeLocationsMap);

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
