using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public IEnumerable<TaskLink> TaskLinks => Tasks.SelectMany(t => Get(TaskFields.PredecessorLinks, t));

        public TaskLink GetTaskLink(TaskId predecessorId, TaskId successorId)
        {
            Debug.Assert(!predecessorId.IsDefault);
            Debug.Assert(!successorId.IsDefault);

            if (!TaskMap.ContainsKey(predecessorId) || !TaskMap.ContainsKey(successorId))
                return null;

            var links = Get(TaskFields.PredecessorLinks, successorId);
            return links.SingleOrDefault(l => l.PredecessorId == predecessorId);
        }

        public IEnumerable<TaskLink> GetTaskLinks(TaskId taskId)
        {
            Debug.Assert(!taskId.IsDefault);

            var predecessorLinks = Get(TaskFields.PredecessorLinks, taskId);
            var successorLinks = Get(TaskFields.SuccessorLinks, taskId);
            return predecessorLinks.Concat(successorLinks);
        }

        public bool TaskLinkCausesCycle(TaskLink taskLink)
        {
            Debug.Assert(taskLink != null);

            if (!TaskMap.ContainsKey(taskLink.PredecessorId) || !TaskMap.ContainsKey(taskLink.SuccessorId))
                return false;

            IEnumerable<TaskId> GetPredecessors(TaskId successorId)
            {
                var links = Get(TaskFields.PredecessorLinks, successorId);

                foreach (var link in links)
                    yield return link.PredecessorId;

                if (successorId == taskLink.SuccessorId)
                    yield return taskLink.PredecessorId;
            }

            var queue = new Queue<TaskId>();

            foreach (var id in Tasks)
            {
                foreach (var predecessorId in GetPredecessors(id))
                {
                    queue.Enqueue(predecessorId);

                    while (queue.Count > 0)
                    {
                        var taskId = queue.Dequeue();
                        if (taskId == id)
                            return true;

                        foreach (var pid in GetPredecessors(taskId))
                            queue.Enqueue(pid);

                        if (taskId == taskLink.SuccessorId)
                            queue.Enqueue(taskLink.PredecessorId);
                    }
                }
            }

            return false;
        }

        public ProjectData AddTaskLink(TaskLink taskLink)
        {
            Debug.Assert(taskLink != null);
            Debug.Assert(GetTaskLink(taskLink.PredecessorId, taskLink.SuccessorId) == null);
            Debug.Assert(!TaskLinkCausesCycle(taskLink));

            var project = this;

            var predescessorLinks = Get(TaskFields.PredecessorLinks, taskLink.SuccessorId);
            predescessorLinks = InsertTaskLink(predescessorLinks, taskLink, isPredecessor: true);

            var successorLinks = Get(TaskFields.SuccessorLinks, taskLink.PredecessorId);
            successorLinks = InsertTaskLink(successorLinks, taskLink, isPredecessor: false);

            project = project.SetRaw(TaskFields.PredecessorLinks, taskLink.SuccessorId, predescessorLinks)
                             .SetRaw(TaskFields.SuccessorLinks, taskLink.PredecessorId, successorLinks)
                             .Reset(TaskFields.Predecessors, taskLink.SuccessorId)
                             .Reset(TaskFields.Successors, taskLink.PredecessorId);

            return project;
        }

        public ProjectData RemoveTaskLink(TaskLink taskLink)
        {
            Debug.Assert(taskLink != null);

            if (!TaskMap.ContainsKey(taskLink.PredecessorId) || !TaskMap.ContainsKey(taskLink.SuccessorId))
                return this;

            var project = this;

            var predescessorLinks = Get(TaskFields.PredecessorLinks, taskLink.SuccessorId);
            predescessorLinks = predescessorLinks.Remove(taskLink);

            var successorLinks = Get(TaskFields.SuccessorLinks, taskLink.PredecessorId);
            successorLinks = successorLinks.Remove(taskLink);

            project = project.SetRaw(TaskFields.PredecessorLinks, taskLink.SuccessorId, predescessorLinks)
                             .SetRaw(TaskFields.SuccessorLinks, taskLink.PredecessorId, successorLinks)
                             .Reset(TaskFields.Predecessors, taskLink.SuccessorId)
                             .Reset(TaskFields.Successors, taskLink.PredecessorId);

            return project;
        }

        private ImmutableArray<TaskLink> InsertTaskLink(ImmutableArray<TaskLink> taskLinks, TaskLink taskLink, bool isPredecessor)
        {
            var newId = isPredecessor ? taskLink.PredecessorId : taskLink.SuccessorId;
            var newOrdinal = Get(TaskFields.Ordinal, newId);

            for (var i = 0; i < taskLinks.Length; i++)
            {
                var existingId = isPredecessor ? taskLinks[i].PredecessorId : taskLinks[i].SuccessorId;
                var existingOrdinal = Get(TaskFields.Ordinal, existingId);

                if (newOrdinal > existingOrdinal)
                    return taskLinks.Insert(i, taskLink);
            }

            return taskLinks.Add(taskLink);
        }
    }
}
