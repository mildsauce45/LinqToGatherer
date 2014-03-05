using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using LinqToGatherer.Utility;

namespace LinqToGatherer
{
    /// <summary>
    /// This is where all the ugly code lies. Mainly because parsing HTML I don't control is never clean
    /// </summary>
    internal class GathererConnection
    {
        private const string GATHERER_SEARCH_BASE_URL = "http://gatherer.wizards.com/Pages/Search/Default.aspx?action=advanced";
        private const string GATHERER_HOME_URL = "http://gatherer.wizards.com/Pages/Default.aspx";
        private const string GATHERER_CARD_IMAGE_URL = "http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid={0}&type=card";

        private static readonly char[] TOKEN_SPLIT_CHARS = new[] { ' ' };
        private static readonly char[] MULTIVERSE_ID_SPLIT_CHARS = new[] { '=' };
        private static readonly char[] PT_SPLIT_CHARS = new[] { '/' };

        [GathererCall(typeof(Card))]
        internal static IEnumerable<Card> GetCards(IList<PropertyQueryInfo> properties)
        {
            // From all the properties that we've determined the query needs to run against
            // group them by the property name so we can add them all on the query string together
            var groupedPropertyAccessors = from p in properties
                                           group p by p.PropertyName into g
                                           select new { Property = g.Key, Tokens = g };

            var queryParams = string.Empty;

            foreach (var group in groupedPropertyAccessors)
                queryParams += string.Format("&{0}={1}", group.Property, GetPropertyQueryParam(group.Tokens.ToList()));

            var url = GATHERER_SEARCH_BASE_URL;

            if (!string.IsNullOrWhiteSpace(queryParams))
                url += queryParams;

            // Get the list of cards at the url we've built up and return them
            return GetCards(url);
        }

        [GathererCall(typeof(string))]
        internal static IEnumerable<string> GetSets(IList<PropertyQueryInfo> properties)
        {
            var web = new HtmlWeb();
            var doc = web.Load(GATHERER_HOME_URL);

            var node = doc.GetElementbyId("ctl00_ctl00_MainContent_Content_SearchControls_setAddText");

            var sets = new List<string>();

            foreach (var option in node.Elements("option"))
            {
                var val = option.Attributes["value"].Value;
                if (!string.IsNullOrWhiteSpace(val))
                    sets.Add(val);
            }

            return sets;
        }

        [GathererCall(typeof(CardImage))]
        internal static IEnumerable<CardImage> GetImages(IList<PropertyQueryInfo> properties)
        {
            var web = new HtmlWeb();
            var results = new List<CardImage>();

            foreach (var propInfo in properties.Where(p => p.PropertyName == "multiverseid"))
            {
                var intValue = propInfo.Value.ToInt();

                // First let's see if we have this card image already cached so we don't hammer gatherer unneccesarily
                if (Gatherer.ImageCache.ContainsKey(intValue))
                {
                    results.Add(Gatherer.ImageCache[intValue]);
                    continue;
                }

                // Looks like the image was not found, so lets ask the web site for the image
                var uri = new Uri(string.Format(GATHERER_CARD_IMAGE_URL, propInfo.Value));

                var imgRequest = WebRequest.Create(uri) as HttpWebRequest;

                var resp = imgRequest.GetResponse();
                var imageStream = resp.GetResponseStream();

                // The image stream here does not support seeking so we need to read from it like a proper stream
                using (var ms = new MemoryStream())
                {
                    int bytesRead;
                    var finished = false;

                    do
                    {
                        var bytes = new byte[64 << 10];
                        bytesRead = imageStream.Read(bytes, 0, bytes.Length);

                        if (bytesRead <= 0)
                            finished = true;
                        else
                            ms.Write(bytes, 0, bytesRead);

                    } while (!finished);

                    imageStream.Close();

                    var cardImage = new CardImage { MultiverseId = propInfo.Value.ToInt(), Image = new ReadOnlyCollection<byte>(ms.ToArray()) };

                    // We know this is a new image so go ahead and add it to the image cache
                    Gatherer.ImageCache.Add(cardImage.MultiverseId, cardImage);

                    results.Add(cardImage);
                }
            }

            return results;
        }

        #region URL Building Helpers

        private static string GetPropertyQueryParam(IEnumerable<PropertyQueryInfo> properties)
        {
            var propertyQueryParam = string.Empty;

            properties.ForEach(p => propertyQueryParam += string.Format("{0}[{1}{2}{3}]", p.Operation, p.WrapInQuotes ? "\"" : string.Empty, HttpUtility.UrlEncode(p.Value), p.WrapInQuotes ? "\"" : string.Empty));

            return propertyQueryParam;
        }

        #endregion

        #region Accessing Resources

        private static IEnumerable<Card> GetCards(string url)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);

            var cards = new List<Card>();

            var singleResult = web.ResponseUri.AbsoluteUri.Contains("Details.aspx");

            // Sometimes the information passed to Gatherer returns the details page of a single card.
            // In this case we need to handle the parsing a little differently
            if (singleResult)
                cards.Add(ParseDetailsPage(doc.DocumentNode, web));
            else
            {
                // Get the cards on the first page
                ParseCardsOnPage(doc.DocumentNode, cards);

                // Now look for the pager
                var pagingDiv = doc.DocumentNode.SelectSingleNode("//div[@class='paging']");

                // See if we can find the next page link to click on, and if so continue clicking and loading pages until there are no more
                var nextPageTag = GetNextPageAnchorTag(pagingDiv);
                while (nextPageTag != null)
                {
                    var uri = new Uri(new Uri("http://gatherer.wizards.com", UriKind.Absolute), nextPageTag.Attributes["href"].Value);

                    url = Uri.UnescapeDataString(uri.AbsoluteUri).Replace("&amp;", "&");

                    // Load the new page
                    doc = web.Load(url);

                    // Parse the new list of cards
                    ParseCardsOnPage(doc.DocumentNode, cards);

                    // Check the pager to see if there's another page to naviagte to
                    pagingDiv = doc.DocumentNode.SelectSingleNode("//div[@class='paging']");
                    nextPageTag = GetNextPageAnchorTag(pagingDiv);
                }
            }

            return cards;
        }

        private static HtmlNode GetNextPageAnchorTag(HtmlNode pagingDiv)
        {
            if (pagingDiv != null)
            {
                foreach (var aNode in pagingDiv.Elements("a"))
                {
                    var text = aNode.InnerText;
                    if (text == "&nbsp;&gt;")
                        return aNode;
                }
            }

            return null;
        }

        private static void ParseCardsOnPage(HtmlNode documentNode, IList<Card> results)
        {
            var cardNodes = new List<HtmlNode>();

            var evenCardNodes = documentNode.SelectNodes("//tr[@class='cardItem evenItem']");
            if (evenCardNodes != null)
                cardNodes.AddRange(evenCardNodes);

            var oddCardNodes = documentNode.SelectNodes("//tr[@class='cardItem oddItem']");
            if (oddCardNodes != null)
                cardNodes.AddRange(oddCardNodes);

            foreach (var node in cardNodes)
            {
                var anchor = node.Descendants("a").FirstOrDefault();

                var hrefText = anchor.Attributes["href"].Value;

                var c = new Card();

                c.MultiverseId = ParseMultiverseId(hrefText);

                var cardTitleNode = node.Descendants("span").FirstOrDefault(n => n.Attributes["class"].Value == "cardTitle");
                anchor = cardTitleNode.Descendants("a").FirstOrDefault();

                c.Name = anchor.InnerText;

                var manaCostNode = node.Descendants("span").FirstOrDefault(n => n.Attributes["class"].Value == "manaCost");

                ParseManaCost(manaCostNode, c);

                var typeLineNode = node.Descendants("span").FirstOrDefault(n => n.Attributes["class"].Value == "typeLine");

                ParseTypeLine(typeLineNode, c);

                var setNode = node.Descendants("td").FirstOrDefault(n => n.Attributes["class"].Value == "rightCol setVersions");

                ParseSet(setNode, c);

                var rulesTextNode = node.Descendants("div").FirstOrDefault(n => n.Attributes["class"].Value == "rulesText");

                c.Description = rulesTextNode != null ? rulesTextNode.InnerText.Trim() : string.Empty;

                results.Add(c);
            }
        }

        private static Card ParseDetailsPage(HtmlNode documentNode, HtmlWeb web)
        {
            var c = new Card();

            // Since details pages just pass along the multiverse id on the url, grab it from there, because we can use the same code we use elsewhere
            c.MultiverseId = ParseMultiverseId(web.ResponseUri.AbsoluteUri);

            var labels = documentNode.SelectNodes("//div[@class='label']");
            foreach (var div in labels)
            {
                switch (div.InnerText.Trim())
                {
                    case "Card Name:":
                        c.Name = div.NextDiv().InnerText.Trim().Replace("|", "/");
                        break;
                    case "Mana Cost:":
                        ParseManaCost(div.NextDiv(), c);
                        break;
                    case "Types:":
                        var types = ParseTypeLineFromString(div.NextDiv().InnerText.CollapseWhitespace().Trim());
                        c.Type = types.Item1;
                        c.SubTypes = types.Item2;
                        break;
                    case "P/T:":
                        var ptString = div.NextDiv().InnerText.Trim();
                        var pt = ptString.Split(PT_SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);

                        c.Power = pt[0].Trim().ToInt();
                        c.Toughness = pt[1].Trim().ToInt();
                        break;
                    case "Expansion:":
                        ParseSet(div.ParentNode, c);
                        break;
                    case "Card Text:":
                        c.Description = div.NextDiv().InnerText.Trim();
                        break;
                }
            }

            return c;
        }

        #endregion

        #region Parsing Methods

        private static int ParseMultiverseId(string text)
        {
            var idRegex = new Regex("multiverseid=(?:[0-9]*)?");

            if (!idRegex.IsMatch(text))
                return 0;

            var parmAndValue = idRegex.Match(text).Value.Split(MULTIVERSE_ID_SPLIT_CHARS);

            return parmAndValue[1].ToInt();
        }

        private static void ParseManaCost(HtmlNode manaCostNode, Card card)
        {
            if (manaCostNode == null)
                return;

            var costNodes = manaCostNode.Descendants("img");

            // '0' isnt a real color, but we can trick .NET into casting it out as one here because these are just ints
            // We'll properly handle the 0 later on
            var color = (Color)0;

            // turns out we can grab the converted mana cost in this part of the code as well. Efficiency!
            int cmc = 0;

            foreach (var imgNode in costNodes)
            {
                if (imgNode.Attributes["alt"] == null || string.IsNullOrWhiteSpace(imgNode.Attributes["alt"].Value))
                    continue;

                var altText = imgNode.Attributes["alt"].Value;

                // If the alt text is just numeric text than its generic and we need to bump the CMC by the int value
                if (altText.IsOnlyNumeric())
                    cmc += altText.ToInt();
                else
                {
                    var colors = altText.Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var c in colors)
                    {
                        // Or the colors together to get the proper value
                        color |= c.ParseDisplayValue<Color>().GetValueOrDefault();

                        // Bump the CMC by one here as this was just a mana symbol
                        cmc++;
                    }
                }
            }

            // If we haven't managed to find a color for this yet, its colorless
            if ((int)color == 0)
                color = Color.Colorless;

            card.Color = color;
            card.CMC = cmc;
        }

        private static void ParseSet(HtmlNode setNode, Card card)
        {
            if (setNode == null)
                return;

            var wrapperDiv = setNode.Descendants("div").ElementAt(1);

            if (wrapperDiv == null)
                return;

            var img = wrapperDiv.Descendants("img").FirstOrDefault();

            if (img == null)
                return;

            var title = img.Attributes["title"].Value;

            var rarityStart = title.IndexOf('(');

            if (rarityStart < 0)
                return;

            // This gets us the set
            card.Set = title.Substring(0, rarityStart).Trim();

            // Now on the result set pages we can get the rarity here as well            
            var rarity = title.Substring(rarityStart + 1);

            card.Rarity = ParseRarity(rarity);
        }

        private static Rarity ParseRarity(string rarity)
        {
            // I'm tired and don't want to do anything more fancy here            
            if (rarity.StartsWith("Common") || rarity.StartsWith("Land"))
                return Rarity.Common;
            else if (rarity.StartsWith("Uncommon"))
                return Rarity.Uncommon;
            else if (rarity.StartsWith("Rare"))
                return Rarity.Rare;
            else if (rarity.StartsWith("Mythic"))
                return Rarity.Mythic;

            return Rarity.Common;
        }

        #region Type Line Parsing Methods

        /// <summary>
        /// Entry point for queries that returned multiple results
        /// </summary>
        private static void ParseTypeLine(HtmlNode typeLineNode, Card card)
        {
            if (typeLineNode == null)
                return;

            // The type line is notoriously rife with odd whitespacing so lets get it down to something a little more managable
            var typeLineText = typeLineNode.InnerText.CollapseWhitespace().Trim();

            var types = ParseTypeLineFromString(typeLineText);

            card.Type = types.Item1;
            card.SubTypes = types.Item2;

            // We can grab the power and toughness from here as well, so let's do that (if it exists)
            var ptStart = typeLineText.IndexOf('(');
            if (ptStart > -1)
            {
                // grab the rest of the power and toughness string
                var ptString = typeLineText.Substring(ptStart + 1);

                // trim off the closing paren
                ptString = ptString.Substring(0, ptString.Length - 1);

                var pt = ptString.Split(new char[] { '/' });

                // * power and toughnesses will be treated as 0 for now
                card.Power = pt[0].ToInt();

                // Currently this handles the planeswalker case where the loyalty is in the parens
                if ((card.Type & CardType.Creature) > 0)
                    card.Toughness = pt[1].ToInt();
            }
        }

        /// <summary>
        /// Primary entry point for the details card page. Also handles the type portion from a result set page
        /// </summary>        
        private static Tuple<CardType, string> ParseTypeLineFromString(string typeLineText)
        {
            var typeParts = typeLineText.Split(TOKEN_SPLIT_CHARS);

            int? dashIndex = null;

            for (var i = 0; i < typeParts.Length; i++)
            {
                if (typeParts[i] == "-" || typeParts[i] == "—")
                {
                    dashIndex = i;
                    break;
                }
            }

            var ct = (CardType)0;

            // Until we see a dash character these are the super types
            foreach (var s in typeParts.Take(dashIndex.HasValue ? dashIndex.Value : typeParts.Length))
            {
                var type = s.ParseDisplayValue<CardType>();

                if (type.HasValue)
                    ct |= type.Value;
            }

            string subTypes = null;

            // Now we can do subtypes for the result set and details pages
            if (dashIndex.HasValue)
            {
                // We can't do power and toughness here because details pages have this info stored elsewhere
                // so lets just grab the sub types and reconstitute them into a single string
                subTypes = string.Join(" ", typeParts.Skip(dashIndex.Value + 1).Where(s => !s.Contains('(')));
            }

            return Tuple.Create(ct, subTypes);
        }

        #endregion

        #endregion
    }
}
