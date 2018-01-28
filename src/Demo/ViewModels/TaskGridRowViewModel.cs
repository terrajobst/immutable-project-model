
using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class TaskGridRowViewModel : ViewModel
    {
        public TaskGridRowViewModel(ProjectWorkspace workspace, TaskId taskId)
        {
            Workspace = workspace;
            TaskId = taskId;
        }

        public ProjectWorkspace Workspace { get; }

        public TaskId TaskId { get; }

        public Task Current => Workspace.Current.GetTask(TaskId);

        public object this[int index]
        {
            get
            {
                var field = TaskFields.All[index];
                return Current.GetValue(field);
            }
            set
            {
                var field = TaskFields.All[index];
                Workspace.ApplyChanges(Current.SetValue(field, value).Project);
            }
        }

        public void Update(TaskChanges changes)
        {
            OnPropertyChanged("Item[]");
        }
    }
}
