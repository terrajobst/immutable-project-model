using System;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class ProjectWorkspace
    {
        public ProjectWorkspace(Project current)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));

            Current = current;
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
