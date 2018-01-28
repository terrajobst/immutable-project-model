using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class TaskGridViewModel : ViewModel
    {
        private Dictionary<TaskId, TaskGridRowViewModel> _rowMap = new Dictionary<TaskId, TaskGridRowViewModel>();

        public TaskGridViewModel(ProjectWorkspace workspace)
        {
            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;
            Rows = new ObservableCollection<TaskGridRowViewModel>();
            Update(Workspace.Current, Workspace.Current.GetChanges(Project.Empty));
        }

        public ProjectWorkspace Workspace { get; }

        public ObservableCollection<TaskGridRowViewModel> Rows { get; }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Update(e.NewProject, e.Changes);
        }

        private void Update(Project project, ProjectChanges changes)
        {
            foreach (var task in changes.AddedTasks.Select(project.GetTask).OrderBy(o => o.Ordinal))
            {
                var row = new TaskGridRowViewModel(Workspace, task.Id);
                Rows.Add(row);
                _rowMap.Add(task.Id, row);
            }

            foreach (var taskId in changes.RemovedTasks)
            {
                Rows.Remove(_rowMap[taskId]);
                _rowMap.Remove(taskId);
            }

            foreach (var taskChanges in changes.ChangedTasks)
                _rowMap[taskChanges.Id].Update(taskChanges);
        }
    }
}
