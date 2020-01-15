using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public class AttributeValueRule : Rule
    {
        public object ExpectedValue { get; set; }
        public string ExpectedKey { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;
            var isValid = false;

            if (nodeToVerify.Attributes != null && nodeToVerify.Attributes.FirstOrDefault(x => x.Key == ExpectedKey) != null)
            {
                var att = nodeToVerify.Attributes.First(x => x.Key == ExpectedKey);
                var val = 0.0d;

                if (double.TryParse(att.Value.ToString(), out val))
                {
                    isValid = val == double.Parse(ExpectedValue.ToString());
                }
                else
                {
                    isValid = att.Value.ToString() == ExpectedValue.ToString();
                }
            }


            if (!isValid)
            {
                nodeToVerify.AddError("RangeRuleError",
                    $"Expected key: '{ExpectedKey}'\n" +
                    $"Expected value: {ExpectedValue}");
            }
            return isValid;
        }
    }
}
