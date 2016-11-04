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
        private INetworkLocationRetriever _networkLocationRetriever;
        private NodeLocationDto[] _nodeLocations;
        private static string WriteFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "testlocation.json");
        private static string ReadFilename => PathUtils.GetFileFullPath("TestFiles" + Path.DirectorySeparatorChar + "testnetworkfilelocation.json");

        [SetUp]
        public void Setup()
        {
            _networkLocationRetriever = new NetworkLocationRetriever();

            _nodeLocations = new[]
            {
                new NodeLocationDto
                {
                    Id = 0,
                    X = 5.0,
                    Y = 6.0
                },
                new NodeLocationDto
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
            var result = _networkLocationRetriever.Read(ReadFilename);

            // Assert
            Assert.That(result.Length, Is.EqualTo(_nodeLocations.Length));

            foreach (var resultNodeLocationDto in result)
            {
                var nodeLocationDto = _nodeLocations.First(n => n.Id == resultNodeLocationDto.Id);

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
            _networkLocationRetriever.Write(WriteFilename, _nodeLocations);

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
