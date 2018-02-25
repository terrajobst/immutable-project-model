using System;
using System.ComponentModel.Composition;

using Demo.Dialogs.ProjectInformation;
using Demo.Services;

namespace Demo.Commands
{
    [Export]
    internal sealed class ProjectInformationCommand : Command
    {
        private readonly WorkspaceService _workspaceService;
        private readonly Lazy<MainWindow> _mainWindow;

        [ImportingConstructor]
        public ProjectInformationCommand(WorkspaceService workspaceService, Lazy<MainWindow> mainWindow)
        {
            _workspaceService = workspaceService;
            _mainWindow = mainWindow;
        }

        public override void Execute(object parameter)
        {
            var dialog = new ProjectInformationDialog(_workspaceService.Current)
            {
                Owner = _mainWindow.Value
            };
            if (dialog.ShowDialog() == true)
                _workspaceService.ApplyChanges(dialog.Project);
        }
    }
}
