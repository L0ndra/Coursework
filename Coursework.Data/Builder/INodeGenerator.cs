using Coursework.Data.Entities;

namespace Coursework.Data.Builder
{
    public interface INodeGenerator
    {
        Node[] GenerateNodes(int count);
        void ResetAccumulator();
    }
}