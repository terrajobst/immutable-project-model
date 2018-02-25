using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Demo.Services;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class ResourceGridViewModel : ViewModel
    {
        private Dictionary<ResourceId, ResourceGridRowViewModel> _rowMap = new Dictionary<ResourceId, ResourceGridRowViewModel>();

        public ResourceGridViewModel(WorkspaceService workspace)
        {
            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;
            Rows = new ObservableCollection<ResourceGridRowViewModel>();
            Update(Workspace.Current, Workspace.Current.GetChanges(Project.Create()));
        }

        public WorkspaceService Workspace { get; }

        public ObservableCollection<ResourceGridRowViewModel> Rows { get; }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            Update(e.NewProject, e.Changes);
        }

        private void Update(Project project, ProjectChanges changes)
        {
            foreach (var resource in changes.AddedResources.Select(project.GetResource))
            {
                var row = new ResourceGridRowViewModel(Workspace, resource.Id);
                Rows.Add(row);
                _rowMap.Add(resource.Id, row);
            }

            foreach (var resourceId in changes.RemovedResources)
            {
                Rows.Remove(_rowMap[resourceId]);
                _rowMap.Remove(resourceId);
            }

            foreach (var resourceChanges in changes.ChangedResources)
                _rowMap[resourceChanges.Id].Update(resourceChanges);
        }
    }

}
