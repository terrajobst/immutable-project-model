using System;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class ProjectChangedEventArgs : EventArgs
    {
        public ProjectChangedEventArgs(Project oldProject, Project newProject, ProjectChanges changes)
        {
            OldProject = oldProject;
            NewProject = newProject;
            Changes = changes;
        }

        public Project OldProject { get; }
        public Project NewProject { get; }
        public ProjectChanges Changes { get; }
    }
}
