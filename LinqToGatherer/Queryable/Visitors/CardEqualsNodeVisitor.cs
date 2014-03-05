using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToGatherer.Queryable.Contracts;
using LinqToGatherer.Utility;

namespace LinqToGatherer.Queryable.Visitors
{
    [NodeType(ExpressionType.Equal)]
    internal class CardEqualsNodeVisitor : INodeTypeVisitor<Card>
    {
        public bool Visit(Expression node, IList<PropertyQueryInfo> urlParams)
        {
            var handled = false;

            if (node.IsMemberValueExpression<Card>("MultiverseId"))
                throw new Exception("You cannot query gatherer with the multiverse id (from the advanced search).");
            else if (node.IsMemberValueExpression<Card>("Name"))
            {
                urlParams.Add(new PropertyQueryInfo("name", node.GetValueFromExpression<Card, string>("Name"), wrapInQuotes: true));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Set"))
            {
                urlParams.Add(new PropertyQueryInfo("set", node.GetValueFromExpression<Card, string>("Set"), wrapInQuotes: true));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Description"))
            {
                urlParams.Add(new PropertyQueryInfo("text", node.GetValueFromExpression<Card, string>("Description"), wrapInQuotes: true));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("SubTypes"))
            {
                urlParams.Add(new PropertyQueryInfo("subtype", node.GetValueFromExpression<Card, string>("SubTypes"), wrapInQuotes: true));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Color"))
            {
                var color = node.GetValueFromExpression<Card, Color>("Color");

                HandleColorExpression(color, urlParams);

                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("CMC"))
            {
                urlParams.Add(new PropertyQueryInfo("cmc", node.GetValueFromExpression<Card, int>("CMC").ToString(), Constants.NumericUrlOperations.EQUALS));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Power"))
            {
                urlParams.Add(new PropertyQueryInfo("power", node.GetValueFromExpression<Card, int>("Power").ToString(), Constants.NumericUrlOperations.EQUALS));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Toughness"))
            {
                urlParams.Add(new PropertyQueryInfo("tough", node.GetValueFromExpression<Card, int>("Toughness").ToString(), Constants.NumericUrlOperations.EQUALS));
                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Type"))
            {
                var type = node.GetValueFromExpression<Card, CardType>("Type");

                HandleTypeExpression(type, urlParams);

                handled = true;
            }
            else if (node.IsMemberValueExpression<Card>("Rarity"))
            {
                urlParams.Add(new PropertyQueryInfo("rarity", node.GetValueFromExpression<Card, Rarity>("Rarity").GetAttribute<GathererValueAttribute>().Value, Constants.StringUrlOperations.OR));

                handled = true;
            }

            return handled;
        }

        private void HandleColorExpression(Color color, IList<PropertyQueryInfo> urlParams)
        {
            var gathererValues = new List<string>();

            var colors = EnumExtensions.GetEnumValues<Color>();

            foreach (var c in colors)
            {
                if ((color & c) > 0)
                    gathererValues.Add(c.GetAttribute<GathererValueAttribute>().Value);
            }

            urlParams.AddRange(gathererValues.Select(gv => new PropertyQueryInfo("color", gv, (gathererValues.Count() > 1 ? Constants.StringUrlOperations.AND : Constants.StringUrlOperations.OR))));
        }

        private void HandleTypeExpression(CardType type, IList<PropertyQueryInfo> urlParams)
        {
            var gathererValues = new List<string>();

            var types = EnumExtensions.GetEnumValues<CardType>();

            foreach (var t in types)
            {
                if ((type & t) > 0)
                    gathererValues.Add(t.ToString());
            }

            urlParams.AddRange(gathererValues.Select(gv => new PropertyQueryInfo("type", gv, (gathererValues.Count() > 1 ? Constants.StringUrlOperations.AND : Constants.StringUrlOperations.OR))));
        }
    }
}
