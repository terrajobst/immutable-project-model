﻿using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        public static readonly FieldKind AssignmentId = new PassthruFieldKind(typeof(AssignmentId));
        public static readonly FieldKind AssignmentIdArray = new PassthruFieldKind(typeof(ImmutableArray<AssignmentId>));
        public static readonly FieldKind ResourceId = new PassthruFieldKind(typeof(ResourceId));
        public static readonly FieldKind TaskId = new PassthruFieldKind(typeof(TaskId));
        public static readonly FieldKind TaskLinkArray = new PassthruFieldKind(typeof(ImmutableArray<TaskLink>));
        public static readonly FieldKind Int32 = new Int32FieldKind();
        public static readonly FieldKind YesNo = new YesNoFieldKind();
        public static readonly FieldKind Text = new TextFieldKind();
        public static readonly FieldKind DateTime = new DateTimeOffsetFieldKind();
        public static readonly FieldKind NullableDateTime = new NullableDateTimeOffsetFieldKind();
        public static readonly FieldKind Work = new WorkFieldKind();
        public static readonly FieldKind Duration = new DurationFieldKind();
        public static readonly FieldKind Percentage = new PercentageFieldKind();
        public static readonly FieldKind ConstraintType = new EnumFieldKind(typeof(ConstraintType));
    }
}
