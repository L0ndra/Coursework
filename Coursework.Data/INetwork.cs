using System.Collections.Generic;
using Coursework.Data.Entities;

namespace Coursework.Data
{
    public interface INetwork
    {
        Node[] Nodes { get; }
        Channel[] Channels { get; }
    }
}