using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace LinqToGatherer
{
    internal static class HtmlParsingExtensions
    {
        public static HtmlNode NextDiv(this HtmlNode element)
        {
            var currNode = element.NextSibling;
            while (currNode != null && currNode.Name != "div")
                currNode = currNode.NextSibling;

            return currNode;
        }
    }
}
