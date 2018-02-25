using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Immutable.ProjectModel;

namespace Demo.Services
{
    [Export]
    internal sealed class UndoRedoService
    {
        private readonly Stack<Project> _undoStack = new Stack<Project>();
        private readonly Stack<Project> _redoStack = new Stack<Project>();

        private bool _isUndoRedoInProgress;

        [ImportingConstructor]
        public UndoRedoService(WorkspaceService workspace)
        {
            Workspace = workspace;
            Workspace.CurrentChanged += Workspace_CurrentChanged;
        }

        public WorkspaceService Workspace { get; }

        public void Reset()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }

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

            OnStateChanged();
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

            OnStateChanged();
        }

        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Workspace_CurrentChanged(object sender, ProjectChangedEventArgs e)
        {
            if (_isUndoRedoInProgress)
                return;

            _undoStack.Push(e.OldProject);
            _redoStack.Clear();

            OnStateChanged();
        }

        public event EventHandler StateChanged;
    }
}
