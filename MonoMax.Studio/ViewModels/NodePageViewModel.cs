using Caliburn.Micro;
using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using MonoMax.Studio.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
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
        private IReadOnlyDictionary<string, string> _componentDetails;

        private string _lastClickedTag;

        public ImageSource Icon { get; }

        public string Header { get; }
        public string[] Filenames { get; }
        public bool IsActive { get; private set; }

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


        public NodePageViewModel(string header, IReadOnlyList<INode> items, ImageSource icon = null)
        {
            Header = header;
            _items = items;
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
                        Items.Filter = x => (x as INode).Tags != null && !_selectedTags.Except((x as INode).Tags).Any();

                    Items.Refresh();
                });

            FindAvailabeNodeCommand = new RelayCommand<INode>(
                (clickedNode) =>
                {
                    var nodes = AppManager.Global.Root
                    .Flatten()
                    .Where(x => x.Header != "Root")
                    .ToList();

                    if (nodes == null || nodes.Count == 0)
                        return;

                    foreach (var n in nodes)
                    {
                        if(n.Validate(n, clickedNode))
                        {
                            var clone = (INode)clickedNode.Clone();
                            n.AddNode(clone);
                            AppManager.Global.NotificationManager.AddNotification($"'{clone.Header}' was added!");
                            break;
                        }
                    }

                    ;
                });

        }

        public ICollectionView AvailableTags { get; private set; }
        public ICollectionView SelectedTags { get; private set; }
        public ICollectionView Items { get; private set; }


        public ICommand LeftDoubleClickCommand { get; }
        public ICommand MouseLeftDownCommand { get; }
        public ICommand FindAvailabeNodeCommand { get; }

        internal void Deactivate()
        {
            Items = null;
            IsActive = false;
            NotifyOfPropertyChange(() => IsActive);
        }

        internal void Activate()
        {
            _componentDetails = GetComponentDetails();
            _availableTags = new List<string>(GetAvailableNodes(_items));
            _selectedTags = new List<string>();

            AvailableTags = CollectionViewSource.GetDefaultView(_availableTags);
            SelectedTags = CollectionViewSource.GetDefaultView(_selectedTags);
            Items = CollectionViewSource.GetDefaultView(_items);

            AvailableTags.SortDescriptions.Add(new SortDescription());
            SelectedTags.SortDescriptions.Add(new SortDescription());

            IsActive = true;
            NotifyOfPropertyChange(() => IsActive);
        }

        private IReadOnlyDictionary<string, string> GetComponentDetails()
        {
            var xlFile = "_itemsdata.xlsx";
            var xlFilePath = Path.Combine(
                new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName, 
                "Data", 
                xlFile);

            if (!File.Exists(xlFilePath))
                return null;


            var dict = new Dictionary<string, string>();

            using (var pck = new ExcelPackage(new FileInfo(xlFilePath)))
            {
                var ws = default(ExcelWorksheet);
                var tbl = default(ExcelTable);

                for (int i = 1; i <= pck.Workbook.Worksheets.Count; i++)
                {
                    ws = pck.Workbook.Worksheets[i];
                    tbl = ws.Tables.First();

                    var start = tbl.Address.Start;
                    var end = tbl.Address.End;

                    var vendor = string.Empty;
                    var id = string.Empty;
                    var description = string.Empty;

                    for (int r = start.Row + 1; r <= end.Row; r++)
                    {
                        id = description = vendor = string.Empty;

                        if(ws.Cells[r, 1].Value != null)
                        {
                            vendor = ws.Cells[r, 1].GetValue<string>();
                            id = ws.Cells[r, 2].GetValue<string>();
                            description = ws.Cells[r, 3].GetValue<string>();

                            var key = vendor + "_" + id;

                            if (!dict.ContainsKey(key))
                                dict.Add(key, description);
                        }

                    }


                }



            }


            return dict;
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
