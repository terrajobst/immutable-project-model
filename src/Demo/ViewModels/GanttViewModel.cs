using System;
using System.Collections.ObjectModel;
using System.Linq;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class GanttViewModel : ViewModel
    {
        public GanttViewModel(ProjectWorkspace workspace)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;

            Tasks = new ObservableCollection<GanttTaskViewModel>();
            Update(workspace.Current, workspace.Current.GetChanges(Project.Create()));
        }

        public ProjectWorkspace Workspace { get; }

        public Project Current { get; private set; }

        public ObservableCollection<GanttTaskViewModel> Tasks { get; }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Update(e.NewProject, e.Changes);
        }

        private void Update(Project project, ProjectChanges changes)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            Current = project;

            foreach (var task in changes.AddedTasks.Select(project.GetTask).OrderBy(o => o.Ordinal))
            {
                var taskViewModel = new GanttTaskViewModel(Workspace, task.Id);
                Tasks.Add(taskViewModel);
            }

            foreach (var taskId in changes.RemovedTasks)
            {
                var taskViewModel = Tasks.SingleOrDefault(vm => vm.Current.Id == taskId);
                if (taskViewModel != null)
                    Tasks.Remove(taskViewModel);
            }

            foreach (var taskChanges in changes.ChangedTasks)
            {
                var taskViewModel = Tasks.SingleOrDefault(vm => vm.Current.Id == taskChanges.Id);
                if (taskViewModel != null)
                    taskViewModel.Update(taskChanges);
            }
        }
    }
}
