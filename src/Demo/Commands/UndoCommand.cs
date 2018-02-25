using System.ComponentModel.Composition;

using Demo.Services;

namespace Demo.Commands
{
    [Export]
    internal sealed class UndoCommand : UndoRedoCommand
    {
        [ImportingConstructor]
        public UndoCommand(UndoRedoService undoRedoService)
            : base(undoRedoService, isUndo: true)
        {
        }
    }

}
