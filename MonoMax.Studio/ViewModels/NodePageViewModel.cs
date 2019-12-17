using Caliburn.Micro;
using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using MonoMax.Studio.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MonoMax.Studio.ViewModels
{
    public class NodePageViewModel : PropertyChangedBase
    {
        private List<string> _availableTags;
        private List<string> _selectedTags;
        private IReadOnlyList<INode> _items;
        
        private string _lastClickedTag;

        public ImageSource Icon { get; }

        public string Header { get; }
        public string Filename { get; }
        public string DefaultPictureFile { get; }

        [Obsolete]
        public NodePageViewModel()
        {
            _items = new List<INode>()
            {
                new Node(){ Key = "MockItem" },
                new Node(){ Key = "MockItem" },
                new Node(){ Key = "MockItem" },
                new Node(){ Key = "MockItem" }
            };

            _availableTags = new List<string>()
            {
                "machine",
                "component",
                "toolholder",
                "workholding",
                "fixtures",
                "static",
                "live tooling",
                "automation",
                "robot"
            };

            _selectedTags = new List<string>()
            {
                "machine",
                "workholding",
                "vdi30"
            };
        }


        public NodePageViewModel(string header, string filename, string defaultPictureFile = "", ImageSource icon = null)
        {
            Header = header;
            Filename = filename;
            DefaultPictureFile = defaultPictureFile;
            Icon = icon;


            MouseLeftDownCommand = new RelayCommand<string>(
                (o) =>
                {
                    _lastClickedTag = o;
                });

            LeftDoubleClickCommand = new RelayCommand<string>(
                (args) =>
                {
                    if(args == "Selected")
                    {
                        _availableTags.Add(_selectedTags.First(x => x == _lastClickedTag));
                        _selectedTags.Remove(_lastClickedTag);
                    }
                    else
                    {
                        _selectedTags.Add(_availableTags.First(x => x == _lastClickedTag));
                        _availableTags.Remove(_lastClickedTag);
                    }

                    AvailableTags.Refresh();
                    SelectedTags.Refresh();

                    if (_selectedTags.Count == 0)
                        Items.Filter = null;
                    else
                        Items.Filter = x => !_selectedTags.Except((x as INode).Tags).Any();

                    Items.Refresh();
                }); 

        }

        public ICollectionView AvailableTags { get; private set; }
        public ICollectionView SelectedTags { get; private set; }
        public ICollectionView Items { get; private set; }


        public ICommand LeftDoubleClickCommand { get; }
        public ICommand MouseLeftDownCommand { get; }


        internal void Deactivate()
        {
            Items = null;

        }

        internal void Activate()
        {
            _items = DeserializeNodes();
            _availableTags = new List<string>(GetAvailableNodes(_items));
            _selectedTags = new List<string>();

            AvailableTags = CollectionViewSource.GetDefaultView(_availableTags);
            SelectedTags = CollectionViewSource.GetDefaultView(_selectedTags);
            Items = CollectionViewSource.GetDefaultView(_items);

            AvailableTags.SortDescriptions.Add(new SortDescription());
            SelectedTags.SortDescriptions.Add(new SortDescription());

        }

        private List<INode> DeserializeNodes()
        {
            var baseDir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;

            var allItems = new List<INode>();
            var file = Path.Combine(baseDir, "Data", Filename);

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

                var items = JsonConvert.DeserializeObject<List<INode>>(File.ReadAllText(file), jsonSettings);

                if (!string.IsNullOrEmpty(DefaultPictureFile))
                    items.ForEach(x => x.ImageSetKey = DefaultPictureFile);

                items.ForEach(x => x.Init());
                allItems.AddRange(items);
            }
           
            return allItems;
        }

        private IReadOnlyList<string> GetAvailableNodes(IReadOnlyList<INode> items)
        {
            if (items == null || items.Count == 0)
                return null;


            var availableTags = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Tags == null || items[i].Tags.Count == 0)
                    continue;

                for (int j = 0; j < items[i].Tags.Count; j++)
                {
                    if (!availableTags.Contains(items[i].Tags[j]))
                        availableTags.Add(items[i].Tags[j]);
                }

            }

            return availableTags.OrderBy(x => x).ToList();
        }

    }
}
