using System.IO;
using Newtonsoft.Json;

namespace Coursework.Data.IONetwork
{
    public class NetworkLocationRetriever : INetworkLocationRetriever
    {
        public NodeLocationDto[] Read(string filename)
        {
            using (var file = new StreamReader(filename))
            {
                var jsonLocations = file.ReadToEnd();

                var result = JsonConvert.DeserializeObject<NodeLocationDto[]>(jsonLocations);

                return result;
            }
        }

        public void Write(string filename, NodeLocationDto[] nodeLocations)
        {
            var jsonLocations = JsonConvert.SerializeObject(nodeLocations, Formatting.Indented);

            using (var file = new StreamWriter(filename))
            {
                file.Write(jsonLocations);
            }
        }
    }
}
