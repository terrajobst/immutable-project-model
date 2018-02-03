using System;

namespace Immutable.ProjectModel
{
    [Flags]
    internal enum FieldFlags
    {
        None              = 0b_00_000,
        Task              = 0b_00_001,
        Resource          = 0b_00_010,
        Assignment        = 0b_00_100,
        Virtual           = 0b_01_000,
        ReadOnly          = 0b_10_000
    }
}
