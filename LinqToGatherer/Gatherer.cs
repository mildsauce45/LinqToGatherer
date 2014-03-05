using System.Linq;
using System.Collections.Generic;

namespace LinqToGatherer
{
    /// <summary>
    /// The class provides all the queryable implementations we support. It also holds a cache of
    /// images we've already requested so we dont need to keep making the same trips across the wire
    /// </summary>
    public static class Gatherer
    {
        public static IQueryable<Card> Cards { get; private set; }
        public static IQueryable<CardImage> CardImages { get; private set; }
        public static IQueryable<string> Sets { get; private set; }

        internal static IDictionary<int, CardImage> ImageCache { get; private set; }
        
        static Gatherer()
        {
            Cards = new GathererQueryable<Card>();
            Sets = new GathererQueryable<string>();
            CardImages = new GathererQueryable<CardImage>();

            ImageCache = new Dictionary<int, CardImage>();
        }
    }
}
