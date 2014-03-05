using System;
using LinqToGatherer.Utility;

namespace LinqToGatherer
{
    public enum Operation
    {
        Or,
        And,
        Not
    }

    [Flags]
    public enum Color
    {
        [GathererValue("C")]
        Colorless = 1,

        [GathererValue("W")]
        White = 2,

        [GathererValue("U")]
        Blue = 4,

        [GathererValue("B")]
        Black = 8,

        [GathererValue("R")]
        Red = 16,

        [GathererValue("G")]
        Green = 32
    }

    /// <summary>
    /// For now we're only supporting Core game cards, not anything like schemes or planes, etc
    /// </summary>
    [Flags]    
    public enum CardType
    {
        Artifact = 1,

        Basic = 2,

        Creature = 4,

        Enchantment = 8,

        Instant = 16,

        Land = 32,

        Legendary = 64,

        Planeswalker = 128,

        Snow = 256,

        Sorcery = 512,

        Tribal = 1024,

        World = 2048
    }

    public enum Rarity
    {
        [GathererValue("C")]
        Common,

        [GathererValue("U")]
        Uncommon,

        [GathererValue("R")]
        Rare,

        [GathererValue("M")]
        Mythic
    }
}
