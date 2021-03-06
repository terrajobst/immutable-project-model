﻿using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class ResourceFields
    {
        public static readonly ResourceField<ResourceId> Id = new ResourceField<ResourceId>("Id", FieldKinds.ResourceId, FieldFlags.ReadOnly);
        public static readonly ResourceField<string> Name = new ResourceField<string>("Name", FieldKinds.Text);
        public static readonly ResourceField<string> Initials = new ResourceField<string>("Initials", FieldKinds.Text);

        internal static readonly ResourceField<ImmutableArray<AssignmentId>> Assignments = new ResourceField<ImmutableArray<AssignmentId>>("Assignments", FieldKinds.AssignmentIdArray, FieldFlags.ReadOnly, ImmutableArray<AssignmentId>.Empty);

        public static readonly ImmutableArray<ResourceField> All = typeof(ResourceFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                         .Select(f => f.GetValue(null))
                                                                                         .OfType<ResourceField>()
                                                                                         .ToImmutableArray();

        public static readonly ImmutableArray<ResourceField> Default = ImmutableArray.Create<ResourceField>(
            Name,
            Initials
        );
    }
}
