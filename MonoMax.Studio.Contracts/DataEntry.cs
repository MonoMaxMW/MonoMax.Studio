using System;
using System.Collections.Generic;
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
}
