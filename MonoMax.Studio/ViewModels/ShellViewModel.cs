using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;
using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using MonoMax.Studio.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
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
        private Node _root = new Node() { Key = "Root", IsExpanded = true, ImageKey = "root.svg" };
        private NodePageViewModel _activeNodePage;
        private Node _selectedNode;

        public ShellViewModel()
        {
            Root = new[] { _root };
            _root.Init();

            NodePagesViewModels = GetNodePages();
            ActiveNodePage = NodePagesViewModels.First();

            DeleteNodeCommand = new RelayCommand<object>(
                    (o) =>
                    {
                        SelectedNode.Parent.RemoveNode(SelectedNode);

                    }, p => SelectedNode != null && SelectedNode.Header != "Root");

            ContextCommands = new[]
            {
                new CommandContainer("Delete", DeleteNodeCommand),

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

        public ICommand DeleteNodeCommand { get; }

        private NodePageViewModel[] GetNodePages()
        {

            var toolingIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/tooling_icon_64.png"));
            var toolholdersIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/toolholders_icon_64.png"));
            var colletIcon = new BitmapImage(new Uri("pack://application:,,,/Assets/collet_icon_64.png"));

            return new[]
            {
                //new NodePageViewModel("Groups", "Groups.json", "package.svg"),
                new NodePageViewModel("Emcoturn 45", new[]{ "Emcoturn45.json" }),
                new NodePageViewModel("Fixtures", new[]{ "Fixtures.json" }),
                new NodePageViewModel("Turrets", new[]{ "Turrets.json" }),

                new NodePageViewModel("Toolholders", new[] { 
                    "Turningtoolholders.json",
                    "Live_toolholders.json"
                }, icon: toolholdersIcon),
                new NodePageViewModel("Turningtools", new[] { 
                    "Shanktools_20x20.json", 
                    "Endmills.json" 
                }, 
                icon: toolingIcon),
                new NodePageViewModel("Collets", new[] { "ER_collets.json" }, icon: colletIcon),

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

    }
}
