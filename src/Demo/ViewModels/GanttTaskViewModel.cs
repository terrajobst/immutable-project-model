using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;

using Demo.Services;

using Immutable.ProjectModel;

using Syncfusion.Windows.Controls.Gantt;

namespace Demo.ViewModels
{
    internal sealed class GanttTaskViewModel : ViewModel
    {
        private bool _syncingPredecessors;

        public GanttTaskViewModel(WorkspaceService workspace, TaskId taskId)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
            TaskId = taskId;
            Predecessors = new ObservableCollection<Predecessor>();
            Predecessors.CollectionChanged += Predecessors_CollectionChanged;

            UpdatePredecessors();
        }

        public WorkspaceService Workspace { get; }

        public TaskId TaskId { get; }

        public Task Current => Workspace.Current.GetTask(TaskId);

        public int Id => Current?.GetValue(TaskFields.Ordinal) ?? 0;

        public string Name
        {
            get => Current.GetValue(TaskFields.Name);
            set => SetField(TaskFields.Name, value);
        }

        public DateTime StartDate
        {
            get => Current.GetValue(TaskFields.Start).LocalDateTime;
            set { }
        }

        public DateTime EndDate
        {
            get => Current.GetValue(TaskFields.Finish).LocalDateTime;
            set { }
        }

        public TimeSpan Duration
        {
            get => Current.GetValue(TaskFields.Duration);
            set => SetField(TaskFields.Duration, value);
        }

        public ObservableCollection<Predecessor> Predecessors { get; }

        public void SetField(TaskField field, object value)
        {
            UpdateProject(Current.SetValue(field, value).Project);
        }

        private void UpdateProject(Project project)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                Workspace.ApplyChanges(project);
            }), DispatcherPriority.ApplicationIdle);
        }

        public void Update(TaskChanges taskChanges)
        {
            if (taskChanges == null)
                throw new ArgumentNullException(nameof(taskChanges));

            foreach (var fieldChange in taskChanges.FieldChanges)
            {
                if (fieldChange.Field == TaskFields.Name)
                    OnPropertyChanged(nameof(Name));
                else if (fieldChange.Field == TaskFields.Start)
                    OnPropertyChanged(nameof(StartDate));
                else if (fieldChange.Field == TaskFields.Finish)
                    OnPropertyChanged(nameof(EndDate));
                else if (fieldChange.Field == TaskFields.Duration)
                    OnPropertyChanged(nameof(Duration));
                else if (fieldChange.Field == TaskFields.Predecessors)
                    UpdatePredecessors();
            }
        }

        private void UpdatePredecessors()
        {
            _syncingPredecessors = true;

            Predecessors.Clear();

            foreach (var task in Current.PredecessorTasks)
            {
                var item = new Predecessor()
                {
                    GanttTaskIndex = task.Ordinal,
                    GanttTaskRelationship = GanttTaskRelationship.FinishToStart,
                    Offset = 0
                };
                Predecessors.Add(item);
            }

            _syncingPredecessors = false;
        }

        private void Predecessors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_syncingPredecessors || Current == null)
                return;

            var ordinals = Predecessors.Select(p => p.GanttTaskIndex);
            var taskByOrdinal = Current.Project.Tasks.ToDictionary(t => t.Ordinal);

            var newPredecessorIds = ordinals.Select(index => taskByOrdinal[index]).Select(t => t.Id).ToImmutableArray();
            var oldPredecessorIds = Current.PredecessorIds.ToImmutableArray();

            if (newPredecessorIds.IsEmpty && oldPredecessorIds.IsEmpty)
                return;

            var toBeRemoved = oldPredecessorIds.Where(i => !newPredecessorIds.Contains(i));
            var toBeAdded = newPredecessorIds.Where(i => !oldPredecessorIds.Contains(i));

            var successorId = Current.Id;
            var project = Current.Project;

            foreach (var predecessorId in toBeRemoved)
                project = project.RemoveTaskLink(predecessorId, successorId);

            foreach (var predecessorId in toBeAdded)
                project = project.AddTaskLink(predecessorId, successorId);

            UpdateProject(project);
        }
    }
}
