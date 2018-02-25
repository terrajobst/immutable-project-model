﻿using System.ComponentModel.Composition;

using Demo.Commands;

namespace Demo.Services
{
    [Export]
    internal sealed class CommandService
    {
        [Import]
        public ExitCommand ExitCommand { get; set; }

        [Import]
        public UndoCommand UndoCommand { get; set; }

        [Import]
        public RedoCommand RedoCommand { get; set; }
    }
}
