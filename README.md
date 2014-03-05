LinqToGatherer
==============

A Linq Query Provider for the Magic: The Gathering Gatherer Database

Have you ever wanted to search some database for a bunch of magic cards fitting some criteria? Come on, of course you have. Well LtG provides the mechanisms to do so without having to maintain your own local database and keep it up to date. We lean on the public face of Gatherer.

#Querying for Cards

Cards are the primary model in LtG. Simply build up a query against the Cards queryable on the Gatherer object in order to retrieve the cards from Gatherer.

## You MUST provide a where clause

Gatherer is a public website, but I'd still like to be a good internet citizen and not constantly request every card from the beginning of time before filtering down further.

Would you like to know how many Clerics there are in the game?

    var cards = (from c in Gatherer.Cards
                 where c.SubTypes.EndsWith("Cleric")
                 orderby c.Name
                 select c).ToList();
                 
or maybe how many mythics there are in the latest set?

    var numCards = (from c in Gatherer.Cards
                    where c.Rarity == Rarity.Mythic && c.Set == "Born of the Gods"
                    select c).Count();
                    
or check out all the red and black cards from Shadowmoor?

    var cards = (from c in Gatherer.Cards
                 where c.Color == (Color.Red | Color.Black) && c.Set == "Shadowmoor"
                 orderby c.Name
                 select c).ToList();
                 
In fact that last query highlights an important part of LinqToGatherer. Compare that list to what is returned within the Gatherer website itself and you'll notice a single card is missing: Reaper King.

## .NET is still in play

The problem with the above query is that Reaper King isn't only red and black...it's every color and that query was looking for cards that are exactly red and black. This is the most important caveat to keep in mind when formulating your queries.

#Other Queryables

There are currently two other queryables in LtG: Sets and CardImages

##Sets

Sets are a very simple model, so simple in fact they are a string.

    var sets = (from s in Gatherer.Sets
                where s.Contains("Ravnica")
                select s).ToList();
                
The only other remarkable thing about Sets is that the Sets queryable is the only queryable that can be queried without a where clause. So the following is a valid LtG query:

    var sets = (from s in Gatherer.Sets select s).ToList();
    
##Card Images

You can also download the bytes for a specific card image. In order to do this though you need to supply the MultiverseId of the image you'd like to download. Any Cards query will give you the MultiverseId of a card or maybe you know it already from Gatherer. Again, since I'd like to be a good internet citizen, these images are cached for the lifetime of your process, so extra queries for the same Multiverse Id don't go over the internet.

    var ids = new List<int> { 600, 601 };
    var imgs = from ci in Gatherer.CardImages
               where ids.Contains(ci.MultiverseId)
               select ci;
    
    
