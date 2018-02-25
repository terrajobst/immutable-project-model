using System.Windows;

using Immutable.ProjectModel;

namespace Demo.Dialogs.ProjectInformation
{
    internal sealed partial class ProjectInformationDialog : Window
    {
        public ProjectInformationDialog(Project project)
        {
            InitializeComponent();
            DataContext = new ProjectInformationViewModel(project);
        }

        private ProjectInformationViewModel ViewModel => (ProjectInformationViewModel)DataContext;

        public Project Project => ViewModel.Project;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
