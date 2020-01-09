using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoMax.Studio.Contracts
{
    [Serializable]
    public class AttributeEntry
    {
        public string Key { get; set; }
        public string Unit { get; set; }
        public object Value { get; set; }
    }

    public interface INode : IDropTarget, ICloneable
    {
        string Header { get; }
        string Text { get; }
        string Key { get; }
        string ImageKey { get; set; }
        List<string> Tags { get; }
        int TreeDepth { get; }
        Dictionary<string, string> Ids { get; set; }
        Dictionary<string, DataEntry> Data { get; set; }
        List<AttributeEntry> Attributes { get; set; }
        IReadOnlyList<INode> ChildNodes { get; }
        IEnumerable<INode> Flatten();
        INode Parent { get; }
        int NodesCount { get; }
        bool IsInitialized { get; }
        void Init();
        void Refresh(int treeDepth = 0);
        bool Validate(params object[] arguments);
        void AddNode(INode node);
        void RemoveNode(INode node);
        void AddText(string languageKey, string text);
    }
}
