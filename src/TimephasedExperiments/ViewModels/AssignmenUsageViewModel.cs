using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

using Immutable.ProjectModel;

using TimephasedExperiments.Contouring;

using Assignment = TimephasedExperiments.Contouring.Assignment;

namespace TimephasedExperiments.ViewModels
{
    internal sealed class AssignmenUsageViewModel : ViewModel
    {
        private static TimeConversion _intervalConversion = TimeConversion.Default.With(24, 7 * 24, 30, TimeUnit.Days, TimeUnit.Hours);

        private Assignment _assignment;
        private DateTime _from;
        private DateTime _to;
        private TimeSpan _interval = _intervalConversion.FromDays(1);
        private List<TimephasedWorkRow> _rows;

        public AssignmenUsageViewModel()
        {
            var start = new DateTime(2018, 2, 5, 8, 0, 0);
            var end = new DateTime(2018, 2, 9, 17, 0, 0);
            var calendar = Calendar.Default;
            _assignment = new Assignment(start, end, TimeSpan.FromHours(40), calendar);
            _assignment.ContourKind = WorkContourKind.FrontLoaded;
            _from = _assignment.Start.LocalDateTime;
            _to = _assignment.End.LocalDateTime;

            var regularContours = Enum.GetValues(typeof(WorkContourKind)).OfType<WorkContourKind>().Where(k => k != WorkContourKind.Contoured);
            ContourKindValues = new ObservableCollection<WorkContourKind>(regularContours);
            UpdateRows();
        }

        public WorkContourKind ContourKind
        {
            get => _assignment.ContourKind;
            set
            {
                if (_assignment.ContourKind != value)
                {
                    _assignment.ContourKind = value;
                    OnPropertyChanged();
                    UpdateToAndRows();
                    UpdateContourKindValues();
                    OnPropertyChanged(nameof(Finish));
                }
            }
        }

        public ObservableCollection<WorkContourKind> ContourKindValues { get; }

        public string Work
        {
            get => TimeConversion.Default.FormatWork(_assignment.Work);
            set
            {
                if (Work != value)
                {
                    if (!TimeConversion.Default.TryParseWork(value, out var work))
                    {
                        MessageBox.Show($"'{value}' isn't a valid value for work.");
                        return;
                    }

                    _assignment.SetWork(work);
                    OnPropertyChanged();
                    UpdateToAndRows();
                    OnPropertyChanged(nameof(Finish));
                }
            }
        }

        public DateTime Start
        {
            get => _assignment.Start.LocalDateTime;
        }

        public DateTime Finish
        {
            get => _assignment.End.LocalDateTime;
        }

        public DateTime From
        {
            get => _from;
            set
            {
                if (_from != value)
                {
                    _from = value;
                    OnPropertyChanged();
                    UpdateRows();
                }
            }
        }

        public DateTime To
        {
            get => _to;
            set
            {
                if (_to != value)
                {
                    _to = value;
                    OnPropertyChanged();
                    UpdateRows();
                }
            }
        }

        public string Interval
        {
            get => _intervalConversion.FormatWork(_interval);
            set
            {
                if (Work != value)
                {
                    if (!_intervalConversion.TryParseWork(value, out var interval))
                    {
                        MessageBox.Show($"'{value}' isn't a valid value for work.");
                        return;
                    }

                    if (interval < TimeSpan.FromMinutes(1))
                    {
                        MessageBox.Show($"The interval cannot be smaller than 1 min");
                        return;
                    }

                    _interval = interval;
                    OnPropertyChanged();
                    UpdateRows();
                }
            }
        }

        public List<TimephasedWorkRow> Rows
        {
            get => _rows;
            set
            {
                if (_rows != value)
                {
                    _rows = value;
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateRows()
        {
            var from = From.Date + Start.TimeOfDay;
            var last = DateTime.MinValue;
            var rows = new List<TimephasedWorkRow>();
            var showTime = (int)_interval.TotalDays != _interval.TotalDays;

            while (from <= To)
            {
                var to = from + _interval;
                var sameDay = last.Date == from.Date;
                var label = showTime
                                ? sameDay ? from.ToLongTimeString() : from.ToString()
                                : from.ToShortDateString();

                var row = new TimephasedWorkRow(label, from, to, _assignment);
                row.PropertyChanged += Row_PropertyChanged;
                rows.Add(row);

                last = from;
                from = to;
            }

            Rows = rows;
        }

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimephasedWorkRow.Work))
            {
                UpdateContourKindValues();

                OnPropertyChanged(nameof(ContourKind));
                OnPropertyChanged(nameof(Work));
                OnPropertyChanged(nameof(Finish));

                foreach (var row in Rows)
                    row.InvalidIsOutside();
            }
        }

        private void UpdateContourKindValues()
        {
            if (_assignment.ContourKind == WorkContourKind.Contoured)
            {
                if (ContourKindValues[0] != WorkContourKind.Contoured)
                    ContourKindValues.Insert(0, WorkContourKind.Contoured);
            }
            else if (ContourKindValues[0] == WorkContourKind.Contoured)
            {
                ContourKindValues.RemoveAt(0);
            }
        }

        private void UpdateTo()
        {
            if (To < _assignment.End.LocalDateTime)
                To = _assignment.End.LocalDateTime;
        }

        private void UpdateToAndRows()
        {
            var rows = Rows;
            UpdateTo();
            if (Rows == rows)
                UpdateRows();
        }
    }
}
