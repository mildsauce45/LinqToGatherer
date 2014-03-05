
namespace LinqToGatherer
{
    internal class Constants
    {
        internal static class StringUrlOperations
        {
            public const string OR = "|";
            public const string AND = "+";
            public const string NOT = "+!";
        }

        internal static class NumericUrlOperations
        {
            public const string EQUALS = "|=";
            public const string NOT_EQUALS = "+!=";
            public const string G_OR_E = "|>=";
            public const string GREATER_THAN = "|>";
            public const string LESS_THAN = "|<";
            public const string L_OR_E = "|<=";            
        }
    }
}
