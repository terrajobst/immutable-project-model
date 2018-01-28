using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class TaskChanges
    {
        internal static TaskChanges Compute(TaskData oldTask, TaskData newTask)
        {
            Debug.Assert(oldTask != newTask);
            Debug.Assert(oldTask.Id == newTask.Id);

            var result = new List<FieldChange<TaskField>>();

            foreach (var field in oldTask.SetFields)
            {
                var oldValue = oldTask.GetValue(field);

                if (!newTask.HasValue(field))
                {
                    result.Add(new FieldChange<TaskField>(field, oldValue, field.DefaultValue));
                }
                else
                {
                    var newValue = newTask.GetValue(field);
                    if (!Equals(oldValue, newValue))
                        result.Add(new FieldChange<TaskField>(field, oldValue, newValue));
                }
            }

            foreach (var field in newTask.SetFields)
            {
                var newValue = newTask.GetValue(field);

                if (!oldTask.HasValue(field))
                {
                    result.Add(new FieldChange<TaskField>(field, field.DefaultValue, newValue));
                }
            }

            return new TaskChanges(oldTask.Id, result.ToImmutableArray());
        }

        private TaskChanges(TaskId id, ImmutableArray<FieldChange<TaskField>> fieldChanges)
        {
            Id = id;
            FieldChanges = fieldChanges;
        }

        public TaskId Id { get; }

        public ImmutableArray<FieldChange<TaskField>> FieldChanges { get; }
    }
}
