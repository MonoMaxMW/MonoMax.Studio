using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public class MaxItemRule : Rule
    {
        public int Value { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;
            var isValid = source.NodesCount < Value; ;

            if (!isValid)
            {
                nodeToVerify.AddError(GetType().Name,
                    $"Maximum amount of elements reached!\n" +
                    $"Allowed amount: {Value}");
            }

            return isValid;
        }
    }
}
