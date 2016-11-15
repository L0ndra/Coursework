using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;

namespace Coursework.Data.Builder
{
    public static class ChannelParamsGenerator
    {
        private static SortedSet<int> UsedPrices { get; } = new SortedSet<int>();

        public static int GetRandomPrice()
        {
            if (!AllConstants.AllPrices.Except(UsedPrices).Any())
            {
                UsedPrices.Clear();
            }

            var price = AllConstants.AllPrices
                .Except(UsedPrices)
                .OrderBy(n => AllConstants.RandomGenerator.Next())
                .FirstOrDefault();

            UsedPrices.Add(price);

            return price;
        }

        public static int GetCapactity(int price)
        {
            var index = AllConstants.AllPrices.IndexOf(price);

            return AllConstants.AllCapacities[index];
        }
    }
}
