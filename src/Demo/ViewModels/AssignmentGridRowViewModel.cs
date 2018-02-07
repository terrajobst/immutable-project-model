
﻿using System;
using System.Windows;

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
                try
                {
                    var project = Current.SetValue(field, value).Project;
                    Workspace.ApplyChanges(project);
                }
                catch (Exception ex)
                {
                    var caption = "Updating Assignment";
                    var message = $"Error setting {field.Name} to '{value}': {ex.Message}";
                    MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void Update(AssignmentChanges changes)
        {
            OnPropertyChanged("Item[]");
        }
    }
}
