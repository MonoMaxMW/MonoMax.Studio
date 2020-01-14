using Caliburn.Micro;
using Microsoft.Win32;
using MonoMax.Studio.Contracts;
using MonoMax.Studio.Contracts.Rules;
using MonoMax.Studio.Internal;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonoMax.Studio.ViewModels
{
    public class GenerateNodedataViewModel : PropertyChangedBase
    {
        public GenerateNodedataViewModel()
        {
            SelectFileCommand = new RelayCommand<object>(
                (o) =>
                {
                    var dlg = new OpenFileDialog()
                    {
                        Filter = "Excel files | *.xlsx"
                    };

                    if ((bool)dlg.ShowDialog())
                    {
                        SelectedFile = dlg.FileName;
                        NotifyOfPropertyChange(() => SelectedFile);
                    }

                });

            RefreshDataCommand = new RelayCommand<object>(
                (o) =>
                {
                    using (var pckg = new ExcelPackage(new FileInfo(SelectedFile)))
                    {
                        var ws = pckg.Workbook.Worksheets.First();
                        var tbl = ws.Tables["tbl_data"];

                        var sr = tbl.Address.Start.Row + 1;
                        var er = tbl.Address.End.Row;
                        var generatedNodes = new List<Node>();
                        var tmpNode = default(Node);

                        var jsonSettings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.Auto,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore,
                            SerializationBinder = new KnownTypesBinder()
                            {
                                KnownTypes = new List<Type>()
                                .Concat(Rule.KnownTypes)
                                .Concat(Node.KnownTypes)
                                .ToList()
                            }
                        };

                        for (int r = sr; r <= er; r++)
                        {
                            var ids = ws.Cells      [r, 1].GetValue<string>();
                            var tags = ws.Cells     [r, 2].GetValue<string>();
                            var imgKey = ws.Cells   [r, 3].GetValue<string>();
                            var enHeader = ws.Cells [r, 4].GetValue<string>();
                            var enText = ws.Cells   [r, 5].GetValue<string>();

                            tmpNode = new Node()
                            {
                                ImageKey = imgKey,
                            };

                            if (!string.IsNullOrEmpty(ids))
                            {
                                var idDict = ids.Split(char.Parse("\n"))
                                .Select(x => new KeyValuePair<string, string>(
                                    x.Substring(0, x.IndexOf(';')),
                                    x.Substring(x.IndexOf(';'), x.Length - x.IndexOf(';')).Replace(";", "")))
                                .ToDictionary(x => x.Key, x => x.Value);
                                tmpNode.Ids = idDict;
                            }

                            if (!string.IsNullOrEmpty(tags))
                            {
                                var itemTags = tags.Split(char.Parse("\n")).OrderBy(x => x).ToList();
                                tmpNode.Tags = itemTags;

                            }

                            tmpNode.Key = enHeader;

                            //tmpNode.Data = 
                            //new Dictionary<string, DataEntry>()
                            //{
                            //    { "en", new DataEntry(){ Header = enHeader, Text = enText} }
                            //};

                            generatedNodes.Add(tmpNode);

                        }

                        GeneratedText = JsonConvert.SerializeObject(generatedNodes, Formatting.Indented, jsonSettings);
                        NotifyOfPropertyChange(() => GeneratedText);
                    }



                }, p => !string.IsNullOrEmpty(SelectedFile));
        }

        public string SelectedFile { get; private set; }
        public string GeneratedText { get; private set; }

        public ICommand SelectFileCommand { get; }
        public ICommand RefreshDataCommand { get; }
    }
}
