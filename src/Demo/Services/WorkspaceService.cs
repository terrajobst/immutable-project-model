using System;
using System.ComponentModel.Composition;

using Immutable.ProjectModel;

namespace Demo.Services
{
    [Export]
    internal sealed class WorkspaceService
    {
        public WorkspaceService()
        {
            Current = Project.Create();
        }

        public Project Current { get; private set; }

        public void ApplyChanges(Project newProject)
        {
            if (newProject == null)
                throw new ArgumentNullException(nameof(newProject));

            var oldProject = Current;

            Current = newProject;

            var changes = newProject.GetChanges(oldProject);
            var args = new ProjectChangedEventArgs(oldProject, newProject, changes);
            CurrentChanged?.Invoke(this, args);
        }

        public event EventHandler<ProjectChangedEventArgs> CurrentChanged;
    }
}
