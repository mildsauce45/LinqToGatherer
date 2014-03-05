using System;
using System.Linq.Expressions;

namespace LinqToGatherer.Utility
{
    /// <summary>
    /// Marks an INodeTypeVisitor with the type of Expression it should be applied to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NodeTypeAttribute : Attribute
    {
        public ExpressionType NodeType { get; private set; }

        public NodeTypeAttribute(ExpressionType nodeType)
        {
            this.NodeType = nodeType;
        }
    }
}
