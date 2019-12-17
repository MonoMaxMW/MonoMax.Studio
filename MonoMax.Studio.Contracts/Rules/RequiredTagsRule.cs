using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public sealed class RequiredTagsRule : Rule
    {
        public List<string> Tags { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;

            if (nodeToVerify == null || nodeToVerify.Tags == null)
                return false;

            var isValid = Tags.All(x => nodeToVerify.Tags.Contains(x));

            if (!isValid)
            {
                nodeToVerify.AddError(GetType().Name,
                    $"REQUIRED TAGS ERROR\n" + 
                    $"Node must include following tags:\n" +
                    $"{string.Join("\n", Tags.Select(x => "# " + x))}");
            }

            return isValid;
        }
    }
}
