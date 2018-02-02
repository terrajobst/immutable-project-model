using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class AssignmentGridViewModel : ViewModel
    {
        private Dictionary<AssignmentId, AssignmentGridRowViewModel> _rowMap = new Dictionary<AssignmentId, AssignmentGridRowViewModel>();

        public AssignmentGridViewModel(ProjectWorkspace workspace)
        {
            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;
            Rows = new ObservableCollection<AssignmentGridRowViewModel>();
            Update(Workspace.Current, Workspace.Current.GetChanges(Project.Create()));
        }

        public ProjectWorkspace Workspace { get; }

        public ObservableCollection<AssignmentGridRowViewModel> Rows { get; }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Update(e.NewProject, e.Changes);
        }

        private void Update(Project project, ProjectChanges changes)
        {
            foreach (var assignment in changes.AddedAssignments
                                              .Select(project.GetAssignment)
                                              .OrderBy(a => a.Task.Ordinal)
                                              .ThenBy(o => o.Resource.Name))
            {
                var row = new AssignmentGridRowViewModel(Workspace, assignment.Id);
                Rows.Add(row);
                _rowMap.Add(assignment.Id, row);
            }

            foreach (var assignmentId in changes.RemovedAssignments)
            {
                Rows.Remove(_rowMap[assignmentId]);
                _rowMap.Remove(assignmentId);
            }

            foreach (var assignmentChanges in changes.ChangedAssignments)
                _rowMap[assignmentChanges.Id].Update(assignmentChanges);
        }
    }
}
