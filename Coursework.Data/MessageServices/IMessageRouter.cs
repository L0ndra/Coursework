using System.Collections.Generic;
using Coursework.Data.Entities;
using Coursework.Data.NetworkData;

namespace Coursework.Data.MessageServices
{
    public interface IMessageRouter
    {
        Channel[] GetRoute(uint senderId, uint receiverId);
        NetworkMatrix CountPriceMatrix(uint currentId, NetworkMatrix matrix = null, 
            SortedSet<uint> visitedNodes = null);
        double CountPrice(uint senderId, uint receiverId);
    }
}
