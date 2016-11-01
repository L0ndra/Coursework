using System.Collections.Generic;
using System.Linq;
using Coursework.Data.NetworkData;

namespace Coursework.Data.Entities
{
    public class NetworkMatrix
    {
        public readonly IDictionary<uint, double> NodeIdWithCurrentPrice = new Dictionary<uint, double>();
        public IDictionary<uint, IDictionary<uint, double>> PriceMatrix 
            = new Dictionary<uint, IDictionary<uint, double>>();

        private NetworkMatrix()
        {
        }

        public static NetworkMatrix Initialize(INetwork network)
        {
            var networkMatrix = new NetworkMatrix();

            foreach (var node in network.Nodes)
            {
                networkMatrix.NodeIdWithCurrentPrice[node.Id] = double.PositiveInfinity;

                networkMatrix.PriceMatrix[node.Id] = new Dictionary<uint, double>
                {
                    [node.Id] = 0.0
                };

                foreach (var node1 in network.Nodes.Where(n => n.Id != node.Id))
                {
                    networkMatrix.PriceMatrix[node.Id][node1.Id] = double.PositiveInfinity;
                }
            }

            return networkMatrix;
        }
    }
}
