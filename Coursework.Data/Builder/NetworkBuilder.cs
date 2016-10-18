namespace Coursework.Data.Builder
{
    public class NetworkBuilder : INetworkBuilder
    {
        private INetwork _network;

        public NetworkBuilder()
        {
            _network = new Network();
        }

        public INetwork Build()
        {
            throw new System.NotImplementedException();
        }

        private void CreateNetwork()
        {
            throw new System.NotImplementedException();
        }
    }
}
