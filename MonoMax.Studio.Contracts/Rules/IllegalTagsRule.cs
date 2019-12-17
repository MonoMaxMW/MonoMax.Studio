using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public sealed class IllegalTagsRule : Rule
    {
        public List<string> Tags { get; set; }

        public override bool Validate(params object[] args)
        {
            var nodeWithRules = args[0] as Node;
            var nodeToVerify = args[1] as Node;

            if (nodeToVerify == null)
                return false;

            var isValid = true;

            if (nodeToVerify.Tags != null)
            {
                for (int i = 0; i < Tags.Count; i++)
                {
                    if (nodeToVerify.Tags.Contains(Tags[i]))
                    {
                        isValid = false;
                        break;
                    }
                }  
            }

            if (!isValid)
                nodeToVerify.AddError(GetType().Name, $"Node has illegal tags!");


            return isValid;
        }
    }
}
