namespace Coursework.Data.Builder
{
    public class MetropolitanNetworkBuilder : INetworkBuilder
    {
        private INetworkBuilder _simpleNetworkBuilder;
        private int _numberOfMetropolitanNetworks;

        public MetropolitanNetworkBuilder(INetworkBuilder simpleNetworkBuilder, int numberOfMetropolitanNetworks)
        {
            _simpleNetworkBuilder = simpleNetworkBuilder;
            _numberOfMetropolitanNetworks = numberOfMetropolitanNetworks;
        }

        public INetworkHandler Build()
        {
            throw new System.NotImplementedException();
        }
    }
}
