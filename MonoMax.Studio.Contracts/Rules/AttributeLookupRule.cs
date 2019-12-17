using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MonoMax.Studio.Contracts.Rules
{
    public enum LookupMode
    {
        Equals,
        Smaller,
        Between
    }

    [Serializable]
    public class LookupEntry
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Mode { get; set; }
    }

    [Serializable]
    public class AttributeLookupRule : Rule
    {
        public LookupEntry[] LookupEntries { get; set; }

        public override bool Validate(params object[] args)
        {
            var results = new List<bool>();
            var model = args[0] as Node;

            foreach (var e in LookupEntries)
            {
                if (model.Attributes == null)
                {
                    results.Add(false);
                    break;
                }

                if (!model.Attributes.ContainsKey(e.Key))
                {
                    results.Add(false);
                }
                else
                {
                    double r, v;

                    switch (e.Mode)
                    {
                        case nameof(LookupMode.Equals):
                            results.Add(e.Value.Equals(model.Attributes[e.Key]));
                            break;

                        case nameof(LookupMode.Smaller):
                            r = e.Value.ToDouble();
                            v = model.Attributes[e.Key].ToDouble();
                            results.Add(v < r);
                            break;


                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            return results.All(x => x);
        }
    }
}
