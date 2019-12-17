using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonoMax.Studio.Internal
{
    public sealed class CommandContainer
    {
        public CommandContainer(string header)
        {
            Header = header;
        }

        public CommandContainer(string header, ICommand command)
            : this(header)
        {
            Command = command;
        }

        public CommandContainer(string header, IEnumerable<CommandContainer> subCommands)
            : this(header)
        {
            ChildCommands = subCommands;
        }

        public string Header { get; }
        public ICommand Command { get; }
        public IEnumerable<CommandContainer> ChildCommands { get; }
    }
}
