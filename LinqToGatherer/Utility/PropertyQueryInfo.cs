
namespace LinqToGatherer
{
    /// <summary>
    /// Helper class that collections and manages how properties should but placed into the main query URL passed to Gatherer
    /// </summary>
    internal class PropertyQueryInfo
    {
        public string PropertyName { get; private set; }
        public string Value { get; private set; }
        public string Operation { get; private set; }
        public bool WrapInQuotes { get; private set; }

        public PropertyQueryInfo(string propertyName, string value, string operation = Constants.StringUrlOperations.OR, bool wrapInQuotes = false)
        {
            this.PropertyName = propertyName;
            this.Value = value;
            this.Operation = operation;
            this.WrapInQuotes = wrapInQuotes;
        }
    }
}
