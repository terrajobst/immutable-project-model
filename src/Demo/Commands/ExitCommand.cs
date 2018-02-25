using System;
using System.ComponentModel.Composition;

namespace Demo.Commands
{
    [Export]
    internal sealed class ExitCommand : Command
    {
        private readonly Lazy<MainWindow> _mainWindow;

        [ImportingConstructor]
        public ExitCommand(Lazy<MainWindow> mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public override void Execute(object parameter)
        {
            _mainWindow.Value.Close();
        }
    }
}
