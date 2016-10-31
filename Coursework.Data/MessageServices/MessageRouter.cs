using System.Linq;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class MessageRouter : SimpleMessageRouter
    {
        public MessageRouter(INetworkHandler network) : base(network)
        {
        }

        protected override double CountPrice(uint startId, uint destinationId)
        {
            var startNode = Network.GetNodeById(startId);
            var destinationNode = Network.GetNodeById(destinationId);

            if (!startNode.IsActive || !destinationNode.IsActive)
            {
                return double.PositiveInfinity;
            }

            var channel = Network.GetChannel(startId, destinationId);

            return channel.Price;
        }
    }
}
