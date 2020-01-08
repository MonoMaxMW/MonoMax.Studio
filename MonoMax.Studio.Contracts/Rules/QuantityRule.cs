using MonoMax.Studio.Contracts.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public class QuantityRule : Rule
    {
        public string Mode { get; set; } = "Smaller";
        public int Value { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;
            var isValid = true;

            switch (Mode)
            {
                case nameof(LookupMode.Smaller):
                case nameof(LookupMode.Equals):
                    isValid = source.NodesCount < Value;
                    break;
            }
             
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
