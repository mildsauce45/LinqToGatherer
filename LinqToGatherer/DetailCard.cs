using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToGatherer
{
    public class DetailCard
    {
        public int MultiverseId { get; internal set; }

        public string Name { get; internal set; }
        public string Description { get; internal set; }
        public string Set { get; internal set; }
        public Color Color { get; internal set; }
        public int CMC { get; internal set; }
        public CardType Type { get; internal set; }
        public string SubTypes { get; internal set; }
        public int? Power { get; internal set; }
        public int? Toughness { get; internal set; }
        public Rarity Rarity { get; internal set; }

        public string Artist { get; internal set; }
    }
}
