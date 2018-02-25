using System.ComponentModel.Composition;

using Demo.Services;

using Immutable.ProjectModel;

namespace Demo.Commands
{
    [Export]
    internal sealed class NewCommand : Command
    {
        private readonly WorkspaceService _workspaceService;

        [ImportingConstructor]
        public NewCommand(WorkspaceService workspaceService)
        {
            _workspaceService = workspaceService;
        }

        public override void Execute(object parameter)
        {
            var newProject = Project.Create();
            _workspaceService.ApplyChanges(newProject);
        }
    }
}
