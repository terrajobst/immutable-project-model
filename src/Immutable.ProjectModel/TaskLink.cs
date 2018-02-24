using System;

namespace Immutable.ProjectModel
{
    public sealed class TaskLink
    {
        public static TaskLink Create(TaskId predecessorId,
                                      TaskId successorId,
                                      TaskLinkType type = TaskLinkType.FinishToStart,
                                      TimeSpan lag = default)
        {
            if (predecessorId.IsDefault)
                throw new ArgumentOutOfRangeException(nameof(predecessorId));

            if (successorId.IsDefault)
                throw new ArgumentOutOfRangeException(nameof(successorId));

            return new TaskLink(predecessorId, successorId, type, lag);
        }

        private readonly TaskId _predecessorId;
        private readonly TaskId _successorId;
        private readonly TaskLinkType _type;
        private readonly TimeSpan _lag;

        private TaskLink(TaskId predecessorId, TaskId successorId, TaskLinkType type, TimeSpan lag)
        {
            _predecessorId = predecessorId;
            _successorId = successorId;
            _type = type;
            _lag = lag;
        }

        public TaskId PredecessorId => _predecessorId;

        public TaskId SuccessorId => _successorId;

        public TaskLinkType Type => _type;

        public TimeSpan Lag => _lag;

        public override string ToString()
        {
            return $"{PredecessorId} -> {SuccessorId} ({_type} +{_lag})";
        }
    }
}
