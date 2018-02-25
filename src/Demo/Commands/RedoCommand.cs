using System.ComponentModel.Composition;

using Demo.Services;

namespace Demo.Commands
{
    [Export]
    internal sealed class RedoCommand : UndoRedoCommand
    {
        [ImportingConstructor]
        public RedoCommand(UndoRedoService undoRedoService)
            : base(undoRedoService, isUndo: false)
        {
        }
    }

}
