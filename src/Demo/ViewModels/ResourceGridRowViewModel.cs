using System;
using System.Windows;

using Demo.Services;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class ResourceGridRowViewModel : ViewModel
    {
        public ResourceGridRowViewModel(WorkspaceService workspace, ResourceId resourceId)
        {
            Workspace = workspace;
            ResourceId = resourceId;
        }

        public WorkspaceService Workspace { get; }

        public ResourceId ResourceId { get; }

        public Resource Current => Workspace.Current.GetResource(ResourceId);

        public object this[int index]
        {
            get
            {
                var field = ResourceFields.All[index];
                return Current.GetValue(field);
            }
            set
            {
                var field = ResourceFields.All[index];
                try
                {
                    var project = Current.SetValue(field, value).Project;
                    Workspace.ApplyChanges(project);
                }
                catch (Exception ex)
                {
                    var caption = "Updating Resource";
                    var message = $"Error setting {field.Name} to '{value}': {ex.Message}";
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Update(ResourceChanges changes)
        {
            OnPropertyChanged("Item[]");
        }
    }
}
