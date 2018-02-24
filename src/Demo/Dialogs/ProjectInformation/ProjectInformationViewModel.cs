using System;

using Demo.ViewModels;

using Immutable.ProjectModel;

using Task = System.Threading.Tasks.Task;

namespace Demo.Dialogs.ProjectInformation
{
    internal sealed class ProjectInformationViewModel : ViewModel
    {
        private Project _project;

        public ProjectInformationViewModel(Project project)
        {
            _project = project;
        }

        public Project Project
        {
            get => _project;
            set
            {
                if (_project != value)
                {
                    _project = value;
                    OnPropertyChanged(nameof(StartDate));
                    OnPropertyChanged(nameof(FinishDate));
                }
            }
        }

        public DateTime StartDate
        {
            get => _project.Start.LocalDateTime;
            set
            {
                if (_project.Start.LocalDateTime != value)
                    UpdateProject(value);
            }
        }

        public DateTime FinishDate => _project.Finish.LocalDateTime;

        private async void UpdateProject(DateTime value)
        {
            Project = await Task.Run(() => Project.WithStartDate(value));
        }
    }
}
