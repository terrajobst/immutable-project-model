
using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class AssignmentGridRowViewModel : ViewModel
    {
        public AssignmentGridRowViewModel(ProjectWorkspace workspace, AssignmentId assignmentId)
        {
            Workspace = workspace;
            AssignmentId = assignmentId;
        }

        public ProjectWorkspace Workspace { get; }

        public AssignmentId AssignmentId { get; }

        public Assignment Current => Workspace.Current.GetAssignment(AssignmentId);

        public object this[int index]
        {
            get
            {
                var field = AssignmentFields.All[index];
                return Current.GetValue(field);
            }
            set
            {
                var field = AssignmentFields.All[index];
                Workspace.ApplyChanges(Current.SetValue(field, value).Project);
            }
        }

        public void Update(AssignmentChanges changes)
        {
            OnPropertyChanged("Item[]");
        }
    }
}
