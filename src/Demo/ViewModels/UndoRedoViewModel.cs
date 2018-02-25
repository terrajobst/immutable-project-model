using System;
using System.Collections.Generic;
using System.Windows.Input;

using Demo.Services;

using Immutable.ProjectModel;

namespace Demo.ViewModels
{
    internal sealed class UndoRedoViewModel
    {
        private readonly Stack<Project> _undoStack = new Stack<Project>();
        private readonly Stack<Project> _redoStack = new Stack<Project>();
        private readonly UndoRedoCommand _undoCommand;
        private readonly UndoRedoCommand _redoCommand;

        private bool _isUndoRedoInProgress;

        public UndoRedoViewModel(WorkspaceService workspace)
        {
            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;
            _undoCommand = new UndoRedoCommand(this, true);
            _redoCommand = new UndoRedoCommand(this, false);
        }

        public WorkspaceService Workspace { get; }

        public void Reset()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            UpdateCommandState();
        }

        public ICommand UndoCommand => _undoCommand;

        public ICommand RedoCommand => _redoCommand;

        public bool CanUndo => _undoStack.Count > 0;

        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo)
                throw new InvalidOperationException();

            _isUndoRedoInProgress = true;
            try
            {
                var newProject = Workspace.Current;
                _redoStack.Push(newProject);

                var oldProject = _undoStack.Pop();
                Workspace.ApplyChanges(oldProject);
            }
            finally
            {
                _isUndoRedoInProgress = false;
            }

            UpdateCommandState();
        }

        public void Redo()
        {
            if (!CanRedo)
                throw new InvalidOperationException();

            _isUndoRedoInProgress = true;
            try
            {
                var oldProject = Workspace.Current;
                _undoStack.Push(oldProject);

                var newProject = _redoStack.Pop();
                Workspace.ApplyChanges(newProject);
            }
            finally
            {
                _isUndoRedoInProgress = false;
            }

            UpdateCommandState();
        }

        private void UpdateCommandState()
        {
            _undoCommand.Changed();
            _redoCommand.Changed();
        }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            if (_isUndoRedoInProgress)
                return;

            _undoStack.Push(e.OldProject);
            _redoStack.Clear();

            UpdateCommandState();
        }

        private sealed class UndoRedoCommand : ICommand
        {
            private readonly UndoRedoViewModel _parent;
            private readonly bool _isUndo;

            public UndoRedoCommand(UndoRedoViewModel parent, bool isUndo)
            {
                _parent = parent;
                _isUndo = isUndo;
            }

            public void Changed()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Execute(object parameter)
            {
                if (_isUndo)
                    _parent.Undo();
                else
                    _parent.Redo();
            }

            public bool CanExecute(object parameter)
            {
                return _isUndo ? _parent.CanUndo : _parent.CanRedo;
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}
