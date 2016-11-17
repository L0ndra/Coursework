using System;
using Coursework.Data.Constants;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public class SimpleMessageRouter : IndependentMessageRouter
    {
        public SimpleMessageRouter(INetworkHandler network) : base(network)
        {
        }
        
        public override double CountPrice(uint startId, uint destinationId)
        {
            var price = base.CountPrice(startId, destinationId);

            if (double.IsInfinity(price) || Math.Abs(price) < AllConstants.Eps)
            {
                return price;
            }

            var channel = Network.GetChannel(startId, destinationId);

            return channel.Price;
        }
    }
}
