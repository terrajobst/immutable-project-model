using System;

namespace Immutable.ProjectModel
{
    public sealed class TaskLink
    {
        public static TaskLink Create(TaskId predecessorId, TaskId successorId)
        {
            if (predecessorId.IsDefault)
                throw new ArgumentOutOfRangeException(nameof(predecessorId));

            if (successorId.IsDefault)
                throw new ArgumentOutOfRangeException(nameof(successorId));

            return new TaskLink(predecessorId, successorId);
        }

        private readonly TaskId _predecessorId;
        private readonly TaskId _successorId;

        private TaskLink(TaskId predecessorId, TaskId successorId)
        {
            _predecessorId = predecessorId;
            _successorId = successorId;
        }

        public TaskId PredecessorId => _predecessorId;

        public TaskId SuccessorId => _successorId;

        public override string ToString()
        {
            return $"{PredecessorId} -> {SuccessorId}";
        }
    }
}
