using System;
using System.Windows;

using Immutable.ProjectModel;

using Assignment = TimephasedExperiments.Contouring.Assignment;

namespace TimephasedExperiments.ViewModels
{
    internal sealed class TimephasedWorkRow : ViewModel
    {
        public TimephasedWorkRow(string label, DateTime from, DateTime to, Assignment assignment)
        {
            Label = label;
            From = from;
            To = to;
            Assignment = assignment;
            IsWorkingTime = assignment.Calendar.GetWork(from, to) > TimeSpan.Zero;
        }

        public DateTime From { get; }
        public DateTime To { get; }
        public Assignment Assignment { get; }

        public bool IsOutside => To < Assignment.Start || From > Assignment.End;
        public bool IsWorkingTime { get; }

        public string Label { get; }

        public string Work
        {
            get => TimeConversion.Default.FormatWork(Assignment.GetWork(From, To));
            set
            {
                if (Work != value)
                {
                    if (To < Assignment.Start)
                    {
                        MessageBox.Show($"You cannot enter work before the assignment's start.");
                        return;
                    }


                    if (!TimeConversion.Default.TryParseWork(value, out var work))
                    {
                        MessageBox.Show($"'{value}' isn't a valid value for work.");
                        return;
                    }

                    try
                    {
                        Assignment.SetWork(From, To, work);
                        OnPropertyChanged();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        public void InvalidIsOutside()
        {
            OnPropertyChanged(nameof(IsOutside));
        }
    }
}
