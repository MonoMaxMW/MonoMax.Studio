using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public class MaxItemTypeRule : Rule
    {
        public string TagName { get; set; }
        public int Value { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var verificatioNode = args[1] as Node;
            var isValid = true;

            if(source.Tags?.Count > 0 && verificatioNode.Tags.Contains(TagName))
            {
                var taggedChildren = source.ChildNodes
                    .Where(x => x.Tags.Contains(TagName))
                    .ToArray();

                isValid = taggedChildren?.Length < Value;
            }

            if (!isValid)
            {
                verificatioNode.AddError(GetType().Name,
                    $"Maximum amount of element types reached!\n" +
                    $"Allowed: '{TagName}' : {Value}");
            }

            return isValid;
        }
    }
}
