using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class AssignmentChanges
    {
        internal static AssignmentChanges Compute(AssignmentData oldAssignment, AssignmentData newAssignment)
        {
            Debug.Assert(oldAssignment != newAssignment);
            Debug.Assert(oldAssignment.Id == newAssignment.Id);

            var result = new List<FieldChange<AssignmentField>>();

            foreach (var field in oldAssignment.SetFields)
            {
                var oldValue = oldAssignment.GetValue(field);

                if (!newAssignment.HasValue(field))
                {
                    result.Add(new FieldChange<AssignmentField>(field, oldValue, field.DefaultValue));
                }
                else
                {
                    var newValue = newAssignment.GetValue(field);
                    if (!Equals(oldValue, newValue))
                        result.Add(new FieldChange<AssignmentField>(field, oldValue, newValue));
                }
            }

            foreach (var field in newAssignment.SetFields)
            {
                var newValue = newAssignment.GetValue(field);

                if (!oldAssignment.HasValue(field))
                {
                    result.Add(new FieldChange<AssignmentField>(field, field.DefaultValue, newValue));
                }
            }

            return new AssignmentChanges(oldAssignment.Id, result.ToImmutableArray());
        }

        private AssignmentChanges(AssignmentId id, ImmutableArray<FieldChange<AssignmentField>> fieldChanges)
        {
            Id = id;
            FieldChanges = fieldChanges;
        }

        public AssignmentId Id { get; }

        public ImmutableArray<FieldChange<AssignmentField>> FieldChanges { get; }
    }

}
