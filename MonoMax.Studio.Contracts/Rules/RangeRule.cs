using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public class RangeRule : Rule
    {
        public string ExpectedKey { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;
            var isValid = false;

            if(nodeToVerify.Attributes != null && nodeToVerify.Attributes.FirstOrDefault(x => x.Key == ExpectedKey) != null)
            {
                var att = nodeToVerify.Attributes.First(x => x.Key == ExpectedKey);
                var val = 0.0d;

                if(double.TryParse(att.Value.ToString(), out val))
                {
                    isValid = val >= MinValue && val <= MaxValue;
                }
            }

            if (!isValid)
            {
                nodeToVerify.AddError("RangeRuleError",
                    $"Expected key: '{ExpectedKey}'\n" +
                    $"Allowed min: {MinValue}\n" +
                    $"Allowed max: {MaxValue}");
            }
            return isValid;
        }
    }
}
