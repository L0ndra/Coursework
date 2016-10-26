using Coursework.Data.Entities;

namespace Coursework.Data.Services
{
    public interface IWideAreaNetworkService
    {
        Node[][] GetNodesInOneMetropolitanNetwork();
    }
}
