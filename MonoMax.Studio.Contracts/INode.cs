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
    public struct DataEntry
    {
        public string Header { get; set; }
        public string Text { get; set; }
    }

    public interface INode : IDropTarget, ICloneable
    {
        string Header { get; }
        string Text { get; }
        string Key { get; }
        string ImageSetKey { get; set; }
        List<string> Tags { get; }
        Dictionary<string, string> Ids { get; set; }
        Dictionary<string, DataEntry> Data { get; set; }
        INode Parent { get; }
        int NodesCount { get; }
        bool IsInitialized { get; }
        void Init();
        bool Validate(params object[] arguments);
        void AddNode(INode node);
        void RemoveNode(INode node);
    }
}
