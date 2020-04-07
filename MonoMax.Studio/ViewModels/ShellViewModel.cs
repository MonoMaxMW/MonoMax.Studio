using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using MonoMax.Studio.Internal;
using MonoMax.Studio.Reports;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MonoMax.Studio.ViewModels
{
    public class KnownTypesBinder : ISerializationBinder
    {
        public IList<Type> KnownTypes { get; set; }

        public Type BindToType(string assemblyName, string typeName)
        {
            return KnownTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }
    }

    [Export(typeof(IShell))]
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private NodePageViewModel _activeNodePage;
        private Node _selectedNode;

        public ShellViewModel()
        {
            NotificationManager = AppManager.Instance.NotificationManager;
            Root = new[] { AppManager.Instance.Root };
            Root.First().Init();

            NodePagesViewModels = GetNodePages();
            ActiveNodePage = NodePagesViewModels.First();

            OpenNodedataGeneratorCommand = new RelayCommand<object>(
                (o) =>
                {
                    var wMgr = IoC.Get<IWindowManager>();
                    wMgr.ShowDialog(new GenerateNodedataViewModel());


                });

            DeleteNodeCommand = new RelayCommand<object>(
                    (o) =>
                    {
                        SelectedNode.Parent.RemoveNode(SelectedNode);

                    }, p => SelectedNode != null && SelectedNode.Header != "Root");

            ContextCommands = new[]
            {
                new CommandContainer("Delete", DeleteNodeCommand),

                new CommandContainer("Generate report", new RelayCommand<object>(
                    (obj) =>
                    {
                        var template = Path.Combine(
                            new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, 
                            "Reports", 
                            "Report_template_letter.xlsx");

                        SelectedNode.Refresh();
                        var items = SelectedNode.Flatten().ToList();
                        ReportGenerator.GenerateExcelReport(template, items);


                    }, p => SelectedNode != null && SelectedNode.Header != "Root")),

                new CommandContainer("Serialize", new RelayCommand<object>(
                    (o) =>
                    {
                        var sf_dlg = new SaveFileDialog()
                        {
                             AddExtension = true,
                             DefaultExt = "mbin",
                             Filter = "Node binary data|*.mbin",
                        };
                        
                        if((bool)sf_dlg.ShowDialog())
                        {
                            var f = sf_dlg.FileName;
                            if(File.Exists(f))
                                File.Delete(f);

                            using(var fs = File.Create(f))
                            {
                                var bw = new BinaryFormatter();
                                bw.Serialize(fs, SelectedNode);
	                        }
                        }

                    }, p => SelectedNode != null && SelectedNode.Header != "Root")),

                new CommandContainer("Deserialize", new RelayCommand<object>(
                    (o) =>
                    {
                        var of_dlg = new OpenFileDialog()
                        {
                             DefaultExt = "mbin",
                             Filter = "Node binary data|*.mbin",
                        };

                        if((bool)of_dlg.ShowDialog())
                        {
                            var f = of_dlg.FileName;
                            if (File.Exists(f))
                            {
                                using(var fs = File.OpenRead(f))
                                {
                                    var bw = new BinaryFormatter();
                                    var node = bw.Deserialize(fs) as Node;

                                    if(SelectedNode.Validate(SelectedNode, node))
                                        SelectedNode.AddNode(node);
                                    else
                                        MessageBox.Show("Deserialization failed!");


                                } 
	                        }
                        }


                    }, p => SelectedNode != null))

            };
        }

        public NotificationManager NotificationManager { get; }
        public ICommand AddSuggestedItemCommand { get; }
        public ICommand DeleteNodeCommand { get; }
        internal ItemRepository ItemRepository { get; } = new ItemRepository();
        public ObservableCollection<INode> InsertableNodes { get; } = new ObservableCollection<INode>();

        public void AddSuggestNode(INode otherNode)
        {
            var clone = otherNode.Clone() as INode;

            if (SelectedNode.Validate(SelectedNode, clone))
            {
                SelectedNode.AddNode(clone);
                AppManager.Instance.NotificationManager.AddNotification($"'{clone.Header}' was added!");

            }
            CheckCanSuggestItems();
        }

        private NodePageViewModel[] GetNodePages()
        {
            var iconAccessories = new BitmapImage(new Uri("pack://application:,,,/Assets/icon_accessories_64.png"));
            var iconFixtures = new BitmapImage(new Uri("pack://application:,,,/Assets/icon_fixtures_64.png"));
            var iconTooling = new BitmapImage(new Uri("pack://application:,,,/Assets/icon_tooling_64.png"));
            var iconToolholders = new BitmapImage(new Uri("pack://application:,,,/Assets/icon_toolholders_64.png"));
            var iconComponents = new BitmapImage(new Uri("pack://application:,,,/Assets/icon_components_64.png"));

            var channels = ItemRepository.DeserializeNodes(new[] { 
                "Channels.json" }
            );

            var fixtures = ItemRepository.DeserializeNodes(new[] {
                "Spindles.json",
                "Fixtures.json"
            });

            var toolholders = ItemRepository.DeserializeNodes(new[] {
                "Toolholders_VDI25.json"
            });

            var tools = ItemRepository.DeserializeNodes(new[] {
                "Tools_Turning.json",
                "Tools_Milling.json",
                "Tools_Drilling.json"
            });

            var accessories = ItemRepository.DeserializeNodes(new[] {
                "Accessory.json"
            });


            return new[]
            {
                new NodePageViewModel("COMPONENTS", channels, iconComponents),
                new NodePageViewModel("FIXTURES", fixtures, iconFixtures),
                new NodePageViewModel("TOOLHOLDERS", toolholders, iconToolholders),
                new NodePageViewModel("TOOLS", tools, iconTooling),
                new NodePageViewModel("ACCESSORIES", accessories, iconAccessories),
            };
        }

        public NodePageViewModel ActiveNodePage
        {
            get { return _activeNodePage; }
            set
            {
                if(value != ActiveNodePage)
                {
                    _activeNodePage?.Deactivate();
                    _activeNodePage = value;
                    _activeNodePage?.Activate();
                    NotifyOfPropertyChange();
                }
            } 
        }

        public void SelectedNodeChanged(RoutedPropertyChangedEventArgs<object> args)
        {
            _selectedNode = args.NewValue as Node;
            NotifyOfPropertyChange(() => SelectedNode);
            NotifyOfPropertyChange(() => SuggestionPopupIsOpen);
        }

        public bool SuggestionPopupIsOpen => CheckCanSuggestItems();

        private bool CheckCanSuggestItems()
        {
            var status = SelectedNode != null && SelectedNode.Header != "Root";
            var foundItems = ItemRepository.FindPossibleChildren(SelectedNode);
            InsertableNodes.Clear();

            if(foundItems.Count > 0)
            {
                for (int i = 0; i < foundItems.Count; i++)
                    InsertableNodes.Add(foundItems[i]);
            }

            return status && foundItems.Count > 0;
        }

        public Node SelectedNode => _selectedNode;

        public void ActivateNodePage(NodePageViewModel nodePage)
        {
            ActiveNodePage = nodePage;
        }

        public NodePageViewModel[] NodePagesViewModels { get; }

        public CommandContainer[] ContextCommands { get; private set; }

        public INode[] Root { get; }
        public ICommand AddNodeCommand { get; }
        public ICommand OpenNodedataGeneratorCommand { get; }

    }
}
