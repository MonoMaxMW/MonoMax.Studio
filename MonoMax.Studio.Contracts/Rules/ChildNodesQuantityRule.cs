using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts.Rules
{
    [Serializable]
    public struct QuantityRuleItem
    {
        public int Value { get; set; }
        public string Key { get; set; }
        public string Id { get; set; }
        public string Mode { get; set; }
    }

    [Serializable]
    public class ChildNodesQuantityRule : Rule
    {
        public List<QuantityRuleItem> Items { get; set; }

        public override bool Validate(params object[] args)
        {
            var source = args[0] as Node;
            var nodeToVerify = args[1] as Node;

            List<bool> validations = new List<bool>();
            
            var items = (source.Nodes.SourceCollection as IEnumerable<INode>).ToList();
            
            for (int i = 0; i < Items.Count; i++)
            {
                var key = Items[i].Key;
                var id = Items[i].Id;
                var value = Items[i].Value; 
                var mode = Items[i].Mode;
                var globCnt = 0;

                for (int j = 0; j < items.Count; j++)
                {
                    var item = items[j];

                    if (item.Ids != null && item.Ids.Count > 0)
                    {
                        globCnt += item.Ids.Count(x => x.Key == key && x.Value == id); ;

                        switch (mode)
                        {
                            case nameof(LookupMode.Smaller):
                                validations.Add(globCnt < value);
                                break;

                            default:
                                throw new NotImplementedException();
                        }  
                    }
                    else
                    {
                        validations.Add(false);
                    }
                }
            }



            var result =  validations.All(x => x == true);

            return result;
        }
    }
}
