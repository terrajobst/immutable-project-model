using System;

namespace Immutable.ProjectModel
{
    [Flags]
    internal enum FieldFlags
    {
        None              = 0b_0_000,
        Task              = 0b_0_001,
        Resource          = 0b_0_010,
        Assignment        = 0b_0_100,
        ReadOnly          = 0b_1_000
    }
}
