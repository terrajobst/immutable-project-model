using System;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public TaskLink GetTaskLink(TaskId predecessorId, TaskId successorId)
        {
            return Data.GetTaskLink(predecessorId, successorId);
        }

        public Project AddTaskLink(TaskLink taskLink)
        {
            if (taskLink == null)
                throw new ArgumentNullException(nameof(taskLink));

            var predecessor = GetTask(taskLink.PredecessorId);
            if (predecessor == null)
                throw new ArgumentException($"The project doesn't contain a task with the predecessor ID {taskLink.PredecessorId}", nameof(taskLink));

            var successor = GetTask(taskLink.SuccessorId);
            if (successor == null)
                throw new ArgumentException($"The project doesn't contain a task with the successor ID {taskLink.SuccessorId}", nameof(taskLink));

            if (Data.TaskLinkCausesCycle(taskLink))
                throw new InvalidOperationException($"Cannot add a link from task {predecessor.Ordinal} to task {successor.Ordinal} as this would cause a cycle.");

            var project = Data.AddTaskLink(taskLink);
            return UpdateProject(project);
        }

        public Project AddTaskLink(TaskId predecessorId, TaskId successorId)
        {
            return AddTaskLink(TaskLink.Create(predecessorId, successorId));
        }

        public Project RemoveTaskLink(TaskLink taskLink)
        {
            if (taskLink == null)
                throw new ArgumentNullException(nameof(taskLink));

            var project = Data.RemoveTaskLink(taskLink);
            return UpdateProject(project);
        }

        public Project RemoveTaskLink(TaskId predecessorId, TaskId successorId)
        {
            var taskLink = GetTaskLink(predecessorId, successorId);
            if (taskLink == null)
                return this;

            return RemoveTaskLink(taskLink);
        }
    }
}
