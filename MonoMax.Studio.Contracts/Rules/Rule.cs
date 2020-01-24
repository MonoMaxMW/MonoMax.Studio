using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MonoMax.Studio.Contracts.Rules
{ 
    [Serializable]
    public abstract class Rule
    {
        public static Type[] KnownTypes = new[]
        {
            typeof(QuantityRule),
            typeof(ChildNodesQuantityRule),
            typeof(RequiredTagsRule),
            typeof(IllegalTagsRule),
            typeof(MaxItemRule),
            typeof(AttributeValueRangeRule),
            typeof(AttributeValueRule),
            typeof(MaxItemTypeRule),

        };

        public abstract bool Validate(params object[] args);
    }
}
