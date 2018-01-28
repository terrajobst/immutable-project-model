using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Threading;

using Immutable.ProjectModel;

using Syncfusion.Windows.Controls.Gantt;

namespace Demo.ViewModels
{
    internal sealed class GanttTaskViewModel : ViewModel
    {
        private bool _syncingPredecessors;

        public GanttTaskViewModel(ProjectWorkspace workspace, TaskId taskId)
        {
            if (workspace == null)
                throw new ArgumentNullException(nameof(workspace));

            Workspace = workspace;
            TaskId = taskId;
            Predecessors = new ObservableCollection<Predecessor>();
            Predecessors.CollectionChanged += Predecessors_CollectionChanged;

            UpdatePrecessors();
        }

        public ProjectWorkspace Workspace { get; }

        public TaskId TaskId { get; }

        public Task Current => Workspace.Current.GetTask(TaskId);

        public int Id => Current.GetValue(TaskFields.Ordinal);

        public string Name
        {
            get => Current.GetValue(TaskFields.Name);
            set => SetField(TaskFields.Name, value);
        }

        public DateTime StartDate
        {
            get => Current.GetValue(TaskFields.EarlyStart).LocalDateTime;
            set { }
        }

        public DateTime EndDate
        {
            get => Current.GetValue(TaskFields.EarlyFinish).LocalDateTime;
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
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var project = Current.SetValue(field, value).Project;
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
                    OnPropertyChanged("Name");
                else if (fieldChange.Field == TaskFields.EarlyStart)
                    OnPropertyChanged("StartDate");
                else if (fieldChange.Field == TaskFields.EarlyFinish)
                    OnPropertyChanged("EndDate");
                else if (fieldChange.Field == TaskFields.Duration)
                    OnPropertyChanged("Duration");
                else if (fieldChange.Field == TaskFields.PredecessorIds)
                    UpdatePrecessors();
            }
        }

        private void UpdatePrecessors()
        {
            _syncingPredecessors = true;

            Predecessors.Clear();

            foreach (var task in Current.Predecessors)
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
            if (_syncingPredecessors)
                return;

            var ordinals = Predecessors.Select(p => p.GanttTaskIndex);
            var taskByOrdinal = Current.Project.Tasks.ToDictionary(t => t.Ordinal);
            var predecessorIds = ordinals.Select(index => taskByOrdinal[index]).Select(t => t.Id).ToImmutableArray();

            if (predecessorIds.IsEmpty && Current.PredecessorIds.IsEmpty)
                return;

            SetField(TaskFields.PredecessorIds, predecessorIds);
        }
    }
}
