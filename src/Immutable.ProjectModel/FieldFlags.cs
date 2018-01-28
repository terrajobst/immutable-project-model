using System;

namespace Immutable.ProjectModel
{
    [Flags]
    internal enum FieldFlags
    {
        None              = 0b_000_000,
        Task              = 0b_000_001,
        Resource          = 0b_000_010,
        Assignment        = 0b_000_100,
        Virtual           = 0b_001_000,
        ReadOnly          = 0b_010_000,
        ImpactsScheduling = 0b_100_000,
    }
}
