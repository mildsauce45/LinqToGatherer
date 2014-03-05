using System;

namespace LinqToGatherer.Utility
{
    /// <summary>
    /// Marks a method with the proper return type so the GathererQueryContext knows which method to invoke on GathererConnection
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class GathererCallAttribute : Attribute
    {
        public Type Type { get; private set; }

        public GathererCallAttribute(Type type)
        {
            this.Type = type;
        }
    }
}
