LinqToGatherer
==============

A Linq Query Provider for the Magic: The Gathering Gatherer Database

Have you ever wanted to search some database for a bunch of magic cards fitting some criteria? Come on, of course you have. Well LtG provides the mechanisms to do so without having to maintain your own local database and keep it up to date. We lean on the public face of Gatherer.

#Querying for Cards

Cards are the primary model in LtG. Simply build up a query against the Cards queryable on the Gatherer object in order to retrieve the cards from Gatherer.

Would you like to know how many Clerics there are in the game?

    var cards = (from c in Gatherer.Cards
                 where c.SubTypes.EndsWith("Cleric")
                 orderby c.Name
                 select c).ToList();
                 
or maybe how many mythics there are in the latest set

    var numCards = (from c in Gatherer.Cards
                    where c.Rarity == Rarity.Mythic && c.Set == "Born of the Gods"
                    select c).Count();
                    
or check out all the red and black cards from Shadowmoor

    var cards = (from c in Gatherer.Cards
                 where c.Color == (Color.Red | Color.Black) && c.Set == "Shadowmoor"
                 orderby c.Name
                 select c).ToList();
                 
in fact that last query highlights an important part of LinqToGatherer. Compare that list to what is returned within the Gatherer website itself and you'll notice a single card is missing: Reaper King.


