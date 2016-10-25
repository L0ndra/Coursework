using System.Collections.Generic;
using System.Linq;
using Coursework.Data.Constants;

namespace Coursework.Data.Builder
{
    public static class PriceGenerator
    {
        private static SortedSet<int> UsedPrices => new SortedSet<int>();

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
    }
}
