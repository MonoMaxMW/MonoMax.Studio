using GongSolutions.Wpf.DragDrop;
using MonoMax.Studio.Contracts.Rules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace MonoMax.Studio.Contracts
{
    public class NodeEqualityComparer : EqualityComparer<INode>
    {
        public override bool Equals(INode x, INode y) => x.GetCompareValue() == y.GetCompareValue();

        public override int GetHashCode(INode obj)
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class Node : INode, INotifyPropertyChanged, IDropTarget
    {
        public static Type[] KnownTypes = new[]
        {
            typeof(Node),
            typeof(SpindleConnection),
        };

        [NonSerialized]
        private Dictionary<string, string> _errors;

        [field: NonSerialized]
        public ICollectionView Errors { get; private set; }

        [field: NonSerialized]
        public IReadOnlyDictionary<int, ImageSource> Images { get; private set; }

        [field: NonSerialized]
        public ICollectionView Nodes { get; private set; }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        [JsonIgnore]
        public IReadOnlyList<INode> ChildNodes => _nodes;

        [field: NonSerialized]
        public bool IsInitialized { get; private set; }

        private bool _isExpanded;
        private List<INode> _nodes = new List<INode>();

        public string Key { get; set; }
        public string ImageKey { get; set; }

        [JsonIgnore]
        public string Header => GetHeader();

        [JsonIgnore]
        public string Text => GetText();
        [JsonIgnore]
        public bool HasText => !string.IsNullOrEmpty(Text);
        public bool HasErrors => _errors?.Count > 0;
        public int NodesCount => _nodes.Count;
        public bool HasImageSet => Images != null && Images.Count == 4;
        public int TreeDepth { get; private set; }
        public int ReportOrder { get; private set; } = 99;

        public void UpdateReportOrder()
        {
            if(Tags != null)
            {
                if (Tags.Contains("Channel") || Tags.Contains("Turret"))
                    ReportOrder = 1;
                else if (Tags.Contains("Fixture") || Tags.Contains("Workholding"))
                    ReportOrder = 2;
                else if (Tags.Contains("Toolholder"))
                    ReportOrder = 3;
                else if (Tags.Contains("Tool") || Tags.Contains("Cuttingtool"))
                    ReportOrder = 4;
            }
        }

        public void Refresh(int treeDepth = 0)
        {
            TreeDepth = treeDepth;

            if (ChildNodes != null)
            {
                foreach (var child in ChildNodes)
                {
                    child.Refresh(child.Parent.TreeDepth + 1);
                }
            }
        }

        public IEnumerable<INode> Flatten()
        {
            yield return this;

            foreach (var node in ChildNodes.SelectMany(childNode => childNode.Flatten()))
            {
                yield return node;
            }
        }

        private string GetText()
        {
            if (Data != null)
            {
                var key = Thread.CurrentThread.GetUiLanguageKey();
                if (Data.ContainsKey(key))
                    return Data[key].Text;
            }

            return string.Empty;
        }

        private string GetHeader()
        {
            if(Data != null)
            {
                var key = Thread.CurrentThread.GetUiLanguageKey();
                if (Data.ContainsKey(key) && !string.IsNullOrEmpty(Data[key].Header))
                    return Data[key].Header;
            }

            return Key;
        }

        public INode Parent { get; private set; }
        public Dictionary<string, string> Ids { get; set; }
        public Dictionary<string, DataEntry> Data { get; set; }
        public Rule[] Rules { get; set; }
        public List<string> Tags { get; set; }
        public List<AttributeEntry> Attributes { get; set; }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if(value != IsExpanded)
                {
                    _isExpanded = value;
                    RaisePropertyChanged();
                }
            }
        }

        internal void AddError(string errorKey, string message)
        {
            if (_errors == null)
            {
                _errors = new Dictionary<string, string>();
                Errors = CollectionViewSource.GetDefaultView(_errors.Values);
            }

            if (_errors.ContainsKey(errorKey))
                return;

            _errors.Add(errorKey, message);                                     
        }

        internal void RemoveError(string errorKey)
        {
            _errors?.Remove(errorKey);
        }

        internal void ClearErrors(string errorKey = "")
        {
            if (_errors == null)
                return;

            if(!string.IsNullOrEmpty(errorKey) && _errors.ContainsKey(errorKey))
                _errors.Remove(errorKey);
            else
                _errors.Clear();
        }

        public void Init()
        {
            if (!string.IsNullOrEmpty(ImageKey))
            {
                var tmpDict = new Dictionary<int, ImageSource>();
                var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                for (int imgSize = 32; imgSize <= 256; imgSize+=imgSize)
                {
                    var imgPath = AssetRepository.GetImagePath(ImageKey, imgSize);

                    if (File.Exists(imgPath))
                        tmpDict.Add(imgSize, new BitmapImage(new Uri(imgPath, UriKind.RelativeOrAbsolute)));

                }

                Images = tmpDict;
                tmpDict = null;
            }

            if (_nodes != null && _nodes.Count > 0)
                _nodes.ForEach(x => x.Init());


            Nodes = CollectionViewSource.GetDefaultView(_nodes);
            IsInitialized = true;
        }

        public override string ToString()
        {
            return $"[{TreeDepth}] {Header}";
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Validate(params object[] arguments)
        {
            var isValid = Header == "Root" ? true : false;
            var nodeToVerify = arguments[1] as Node;
            nodeToVerify.ClearErrors();

            if (Rules != null)
                isValid = Rules.All(x => x.Validate(arguments));
            else if (Rules == null && Header != "Root")
                nodeToVerify.AddError("NoChildNodesAllowedError", "Node doesn't allow child nodes!");


            return isValid;
        }

        public void AddNode(INode node)
        {
            if (node == null)
                throw new NullReferenceException();
            
            if (!node.IsInitialized)
                node.Init();

            var n = (Node)node;
            n.Parent = this;

            _nodes.Add(n);
            Nodes.Refresh();
            IsExpanded = true;
            n = null;
        }

        public void RemoveNode(INode node)
        {
            if (_nodes.Contains(node))
            {
                _nodes.Remove(node);
                Nodes.Refresh();
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var draggedNode = dropInfo.Data as Node;
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;

            if (Validate(this, draggedNode))
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var clone = (dropInfo.Data as INode).Clone() as INode;
            this.AddNode(clone);

        }        

        public string GetCompareValue()
        {
            var idItem = Ids != null ? Ids.First().ToString() : string.Empty;
            var key = Header;

            return string.IsNullOrEmpty(idItem) ? key : idItem;
        }

        public void AddText(string languageKey, string text)
        {
            if (Data == null)
                Data = new Dictionary<string, DataEntry>();

            Data.Add(languageKey, new DataEntry() { Text = text });
        }

        public virtual object Clone()
        {
            var clonedNode = new Node()
            {
                Key = this.Key,
                ImageKey = this.ImageKey,
                Rules = this.Rules,
                Data = this.Data,
                Tags = this.Tags,
                Ids = this.Ids,
                Attributes = this.Attributes
            };

            clonedNode.Init();

            return clonedNode;
        }
    }
}
