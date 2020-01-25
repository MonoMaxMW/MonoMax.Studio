using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.ViewModels
{
    internal class ItemRepository
    {
        private readonly List<INode> _items = new List<INode>();

        internal IReadOnlyList<INode> AllItems => _items;

        internal List<INode> DeserializeNodes(string[] filesNames)
        {
            var allItems = new List<INode>();

            for (int i = 0; i < filesNames.Length; i++)
            {
                var file = AssetRepository.GetFullAssetpath(filesNames[i]);

                if (File.Exists(file))
                {
                    var jsonSettings = new JsonSerializerSettings()
                    {
                        TypeNameHandling = TypeNameHandling.Auto,
                        SerializationBinder = new KnownTypesBinder()
                        {
                            KnownTypes = new List<Type>()
                                .Concat(Rule.KnownTypes)
                                .Concat(Node.KnownTypes)
                                .ToList()
                        }
                    };

                    var items = JsonConvert.DeserializeObject<List<Node>>(File.ReadAllText(file), jsonSettings);

                    items.ForEach(x => x.Init());
                    allItems.AddRange(items);
                }
            }

            _items.AddRange(allItems);
            return allItems;
        }

        internal List<INode> FindPossibleChildren(INode node)
        {
            var foundNodes = new List<INode>();

            if(AllItems.Count > 0 && node != null)
            {
                foreach (var n in AllItems)
                {
                    if(node.Validate(node, n))
                    {
                        foundNodes.Add(n);
                    }
                }
            }

            return foundNodes;
        }
    }
}
