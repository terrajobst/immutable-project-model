using Demo.Services;

namespace Demo.Commands
{
    internal abstract class UndoRedoCommand : Command
    {
        private readonly UndoRedoService _undoRedoService;
        private readonly bool _isUndo;

        protected UndoRedoCommand(UndoRedoService undoRedoService, bool isUndo)
        {
            _undoRedoService = undoRedoService;
            _undoRedoService.StateChanged += (s, e) => OnCanExecuteChanged();
            _isUndo = isUndo;
        }

        public override bool CanExecute(object parameter)
        {
            return _isUndo ? _undoRedoService.CanUndo : _undoRedoService.CanRedo;
        }

        public override void Execute(object parameter)
        {
            if (_isUndo)
                _undoRedoService.Undo();
            else
                _undoRedoService.Redo();
        }
    }
}
