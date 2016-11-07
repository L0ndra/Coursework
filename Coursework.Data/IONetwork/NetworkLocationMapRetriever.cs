using System.IO;
using Newtonsoft.Json;

namespace Coursework.Data.IONetwork
{
    public class NetworkLocationMapRetriever : INetworkLocationMapRetriever
    {
        public NodeLocationMapDto[] Read(string filename)
        {
            using (var file = new StreamReader(filename))
            {
                var jsonLocations = file.ReadToEnd();

                var result = JsonConvert.DeserializeObject<NodeLocationMapDto[]>(jsonLocations);

                return result;
            }
        }

        public void Write(string filename, NodeLocationMapDto[] nodeLocationsMap)
        {
            var jsonLocations = JsonConvert.SerializeObject(nodeLocationsMap, Formatting.Indented);

            using (var file = new StreamWriter(filename))
            {
                file.Write(jsonLocations);
            }
        }
    }
}
