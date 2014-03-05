using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToGatherer.Utility
{
    /// <summary>
    /// The raw enum values aren't always what should be passed to gatherer, this attribute gives us a place to store the values that should be
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
    public class GathererValueAttribute : Attribute
    {
        public string Value { get; private set; }

        public GathererValueAttribute(string value)
        {
            this.Value = value;
        }
    }
}
