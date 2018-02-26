using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<TaskId, TaskData> TaskMap => _taskMap;

        public IEnumerable<TaskId> Tasks => _taskMap.Keys;

        public TaskId GetTask(int ordinal)
        {
            return _taskMap.Keys.SingleOrDefault(t => Get(TaskFields.Ordinal, t) == ordinal);
        }

        public IEnumerable<TaskId> GetTasks(ResourceId resourceId)
        {
            return _assignmentMap.Keys.Where(a => Get(AssignmentFields.ResourceId, a) == resourceId)
                                    .Select(a => Get(AssignmentFields.TaskId, a));
        }

        public IEnumerable<TaskId> GetPredecessors(TaskId taskId)
        {
            return Get(TaskFields.PredecessorLinks, taskId).Select(l => l.PredecessorId);
        }

        public IEnumerable<TaskId> GetSuccessors(TaskId taskId)
        {
            return Get(TaskFields.SuccessorLinks, taskId).Select(l => l.SuccessorId);
        }

        private ProjectData WithTaskMap(ImmutableDictionary<TaskId, TaskData> taskMap)
        {
            Debug.Assert(taskMap != null);

            return With(_information, taskMap, _resourceMap, _assignmentMap);
        }

        public ProjectData AddTask(TaskId taskId)
        {
            Debug.Assert(!taskId.IsDefault);
            Debug.Assert(!_taskMap.ContainsKey(taskId));

            var task = TaskData.Create(taskId).SetValue(TaskFields.Ordinal, _taskMap.Count);
            var tasks = _taskMap.Add(task.Id, task);
            return WithTaskMap(tasks);
        }

        public ProjectData RemoveTask(TaskId taskId)
        {
            Debug.Assert(!taskId.IsDefault);

            var project = this;

            // Avoid cascading errors when we remove tasks that don't exist

            if (!_taskMap.ContainsKey(taskId))
                return project;

            // Remove assignments

            foreach (var assignmentId in project.GetAssignments(taskId))
                project = project.RemoveAssignment(assignmentId);

            // Remove task links

            foreach (var taskLink in GetTaskLinks(taskId))
                project = project.RemoveTaskLink(taskLink);

            // Remove task

            project = project.WithTaskMap(project._taskMap.Remove(taskId));

            // Update tasks

            var sortedTaskIds = project.Tasks
                                       .OrderBy(t => project.Get(TaskFields.Ordinal, t))
                                       .ToArray();

            for (var i = 0; i < sortedTaskIds.Length; i++)
            {
                var id = sortedTaskIds[i];

                // Update ordinal
                var ordinal = i;
                project = project.SetRaw(TaskFields.Ordinal, id, ordinal);
            }

            return project;
        }

        public T Get<T>(TaskField<T> field, TaskId id)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            return _taskMap[id].GetValue(field);
        }

        public ProjectData Set(TaskField field, TaskId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            if (field == TaskFields.Ordinal)
            {
                return SetTaskOrdinal(this, id, (int)value);
            }
            else if (field == TaskFields.Name)
            {
                return SetTaskName(this, id, (string)value);
            }
            else if (field == TaskFields.Duration)
            {
                return SetTaskDuration(this, id, (TimeSpan)value);
            }
            else if (field == TaskFields.Work)
            {
                return SetTaskWork(this, id, (TimeSpan)value);
            }
            else if (field == TaskFields.Predecessors)
            {
                return SetTaskPredecessors(this, id, (string)value);
            }
            else if (field == TaskFields.Successors)
            {
                return SetTaskSuccessors(this, id, (string)value);
            }
            else if (field == TaskFields.ResourceNames)
            {
                return SetTaskResourceNames(this, id, (string)value);
            }
            else if (field == TaskFields.ResourceInitials)
            {
                return SetTaskResourceInitials(this, id, (string)value);
            }
            else
            {
                return SetRaw(field, id, value);
            }
        }

        public ProjectData SetRaw(TaskField field, TaskId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            var task = _taskMap[id].SetValue(field, value);
            return WithTaskMap(_taskMap.SetItem(task.Id, task));
        }

        public ProjectData Reset(TaskField field, TaskId id)
        {
            if (field == TaskFields.Predecessors)
            {
                return ResetTaskPredecessors(this, id);
            }
            else if (field == TaskFields.Successors)
            {
                return ResetTaskSuccessors(this, id);
            }
            else if (field == TaskFields.ResourceNames)
            {
                return ResetTaskResourceNames(this, id);
            }
            else if (field == TaskFields.ResourceInitials)
            {
                return ResetTaskResourceInitials(this, id);
            }
            else
            {
                Debug.Assert(false, $"Unexpected field passed to Reset: {field}");
                return this;
            }
        }

        private static ProjectData SetTaskOrdinal(ProjectData project, TaskId id, int value)
        {
            if (value < 0 || value >= project.TaskMap.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            var oldOrdinal = project.Get(TaskFields.Ordinal, id);
            var newOrdinal = value;

            var orderedTaskIds = project.Tasks
                                        .OrderBy(t => project.Get(TaskFields.Ordinal, t))
                                        .ToList();

            orderedTaskIds.RemoveAt(oldOrdinal);
            orderedTaskIds.Insert(newOrdinal, id);

            // First we update all ordinals

            for (var i = 0; i < orderedTaskIds.Count; i++)
            {
                var taskId = orderedTaskIds[i];
                var ordinal = i;

                project = project.SetRaw(TaskFields.Ordinal, taskId, ordinal);
            }

            // Then we can update all predecessors/successors

            foreach (var taskId in orderedTaskIds)
                project = project.Reset(TaskFields.Predecessors, taskId)
                                 .Reset(TaskFields.Successors, taskId);

            return project;
        }

        private static ProjectData SetTaskName(ProjectData project, TaskId id, string value)
        {
            var assignmentIds = project.GetAssignments(id);

            project = project.SetRaw(TaskFields.Name, id, value);

            foreach (var assignmentId in assignmentIds)
                project = project.Set(AssignmentFields.TaskName, assignmentId, value);

            return project;
        }

        private static ProjectData SetTaskDuration(ProjectData project, TaskId id, TimeSpan value)
        {
            project = project.SetRaw(TaskFields.Duration, id, value);

            var taskFinish = project.Get(TaskFields.Finish, id);

            foreach (var assignmentId in project.GetAssignments(id))
            {
                var assignmentFinish = project.Get(AssignmentFields.Finish, assignmentId);
                var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                if (assignmentFinish == taskFinish)
                {
                    var assignmentWork = TimeSpan.FromHours(value.TotalHours * assignmentUnits);
                    project = project.Set(AssignmentFields.Work, assignmentId, assignmentWork);
                }
            }

            return project;
        }

        private static ProjectData SetTaskWork(ProjectData project, TaskId id, TimeSpan value)
        {
            project = project.SetRaw(TaskFields.Work, id, value);

            if (value == TimeSpan.Zero)
            {
                foreach (var assignmentId in project.GetAssignments(id))
                    project = project.SetRaw(AssignmentFields.Work, assignmentId, TimeSpan.Zero);
            }
            else
            {
                var totalExistingWork = TimeSpan.Zero;

                foreach (var assignmentId in project.GetAssignments(id))
                    totalExistingWork += project.Get(AssignmentFields.Work, assignmentId);

                var assignmentCount = project.GetAssignments(id).Count();

                foreach (var assignmentId in project.GetAssignments(id))
                {
                    var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);

                    double newHours;

                    if (totalExistingWork > TimeSpan.Zero)
                    {
                        newHours = assignmentWork.TotalHours / totalExistingWork.TotalHours * value.TotalHours;
                    }
                    else
                    {
                        newHours = value.TotalHours / assignmentCount;
                    }

                    var newWork = TimeSpan.FromHours(newHours);

                    project = project.SetRaw(AssignmentFields.Work, assignmentId, newWork);
                }
            }

            return project;
        }

        private static ProjectData ResetTaskPredecessors(ProjectData project, TaskId id)
        {
            return ResetTaskPredecessorsOrSuccessors(project, id, isSuccessors: false);
        }

        private static ProjectData SetTaskPredecessors(ProjectData project, TaskId id, string value)
        {
            return SetTaskPredecessorsOrSuccessors(project, id, value, isSuccessors: false);
        }

        private static ProjectData ResetTaskSuccessors(ProjectData project, TaskId id)
        {
            return ResetTaskPredecessorsOrSuccessors(project, id, isSuccessors: true);
        }

        private static ProjectData SetTaskSuccessors(ProjectData project, TaskId id, string value)
        {
            return SetTaskPredecessorsOrSuccessors(project, id, value, isSuccessors: true);
        }

        private static ProjectData ResetTaskPredecessorsOrSuccessors(ProjectData project, TaskId id, bool isSuccessors)
        {
            TaskId GetPredecessor(TaskLink l) => l.PredecessorId;
            TaskId GetSuccessor(TaskLink l) => l.SuccessorId;

            var getTask = isSuccessors ? (Func<TaskLink, TaskId>)GetSuccessor : GetPredecessor;
            var field = isSuccessors ? TaskFields.Successors : TaskFields.Predecessors;
            var linkField = isSuccessors ? TaskFields.SuccessorLinks : TaskFields.PredecessorLinks;

            var sb = new StringBuilder();

            var predecessors = project.Get(linkField, id);

            foreach (var p in predecessors.OrderBy(p => project.Get(TaskFields.Ordinal, getTask(p))))
            {
                if (sb.Length > 0)
                    sb.Append(",");

                var ordinal = project.Get(TaskFields.Ordinal, getTask(p));
                sb.Append(ordinal);

                switch (p.Type)
                {
                    case TaskLinkType.FinishToStart:
                        if (p.Lag != TimeSpan.Zero)
                            sb.Append("FS");
                        break;
                    case TaskLinkType.StartToStart:
                        sb.Append("SS");
                        break;
                    case TaskLinkType.FinishToFinish:
                        sb.Append("FF");
                        break;
                    case TaskLinkType.StartToFinish:
                        sb.Append("SF");
                        break;
                }

                if (p.Lag != TimeSpan.Zero)
                {
                    if (p.Lag > TimeSpan.Zero)
                        sb.Append("+");
                    var lag = project.Information.TimeConversion.FormatDuration(p.Lag);
                    sb.Append(lag);
                }
            }

            var precedessors = sb.ToString();
            return project.SetRaw(field, id, precedessors);
        }

        private static ProjectData SetTaskPredecessorsOrSuccessors(ProjectData project, TaskId id, string value, bool isSuccessors)
        {
            var linkField = isSuccessors ? TaskFields.SuccessorLinks : TaskFields.PredecessorLinks;

            value = value.Trim();

            var predecessorsBuilder = ImmutableArray.CreateBuilder<TaskId>();
            var remainingTaskLinks = project.Get(linkField, id).ToList();

            if (value.Length > 0)
            {
                var parts = value.Split(',');

                foreach (var part in parts)
                {
                    var partText = part.Trim();

                    var taskText = partText;
                    var sign = Math.Max(partText.IndexOf('+'), partText.IndexOf('-'));
                    var lag = TimeSpan.Zero;

                    if (sign >= 0)
                    {
                        var lagText = partText.Substring(sign).Trim();
                        lag = project.Information.TimeConversion.ParseDuration(lagText);
                        taskText = partText.Substring(0, sign).Trim();
                    }

                    var taskOrdinalText = taskText;
                    var linkType = (TaskLinkType?)null;

                    if (taskText.EndsWith("FS", StringComparison.OrdinalIgnoreCase))
                        linkType = TaskLinkType.FinishToStart;
                    else if (taskText.EndsWith("SS", StringComparison.OrdinalIgnoreCase))
                        linkType = TaskLinkType.StartToStart;
                    else if (taskText.EndsWith("FF", StringComparison.OrdinalIgnoreCase))
                        linkType = TaskLinkType.FinishToFinish;
                    else if (taskText.EndsWith("SF", StringComparison.OrdinalIgnoreCase))
                        linkType = TaskLinkType.StartToFinish;

                    if (linkType != null)
                        taskOrdinalText = taskText.Substring(0, taskText.Length - 2).Trim();
                    else if (sign < 0)
                        linkType = TaskLinkType.FinishToStart;
                    else
                        throw new FormatException($"'{partText}' isn't a valid value");

                    if (!int.TryParse(taskOrdinalText, out var taskOrdinal))
                        throw new FormatException($"'{taskOrdinalText}' isn't a valid int");

                    var taskId = project.GetTask(taskOrdinal);
                    if (taskId.IsDefault)
                        throw new FormatException($"'{taskOrdinal}' isn't a valid task");

                    var predecessorId = isSuccessors ? id : taskId;
                    var successorId = isSuccessors ? taskId : id;

                    var taskLink = TaskLink.Create(predecessorId, successorId, linkType.Value, lag);

                    var existingLink = project.GetTaskLink(predecessorId, successorId);
                    if (existingLink != null)
                    {
                        project = project.RemoveTaskLink(existingLink);
                    }
                    else if (project.TaskLinkCausesCycle(taskLink))
                    {
                        var predecessorOrdinal = project.Get(TaskFields.Ordinal, predecessorId);
                        var successorOrdinal = project.Get(TaskFields.Ordinal, successorId);
                        throw new InvalidOperationException($"Cannot add a link from task {predecessorOrdinal} to task {successorOrdinal} as this would cause a cycle.");
                    }

                    project = project.AddTaskLink(taskLink);

                    remainingTaskLinks.Remove(taskLink);
                }
            }

            foreach (var taskLink in remainingTaskLinks)
                project = project.RemoveTaskLink(taskLink);

            return project;
        }

        private static ProjectData ResetTaskResourceNames(ProjectData project, TaskId id)
        {
            return ResetTaskResourceNamesOrInitials(project, id, isInitials: false);
        }

        private static ProjectData SetTaskResourceNames(ProjectData project, TaskId id, string value)
        {
            return SetTaskResourceNamesOrInitials(project, id, value, isInitials: false);
        }

        private static ProjectData ResetTaskResourceInitials(ProjectData project, TaskId id)
        {
            return ResetTaskResourceNamesOrInitials(project, id, isInitials: true);
        }

        private static ProjectData SetTaskResourceInitials(ProjectData project, TaskId id, string value)
        {
            return SetTaskResourceNamesOrInitials(project, id, value, isInitials: true);
        }

        private static ProjectData ResetTaskResourceNamesOrInitials(ProjectData project, TaskId id, bool isInitials)
        {
            var resourceField = isInitials ? ResourceFields.Initials : ResourceFields.Name;
            var taskField = isInitials ? TaskFields.ResourceInitials : TaskFields.ResourceNames;

            var sb = new StringBuilder();

            foreach (var a in project.GetAssignments(id)
                                     .OrderBy(a => project.Get(resourceField, project.Get(AssignmentFields.ResourceId, a))))
            {
                var resourceId = project.Get(AssignmentFields.ResourceId, a);
                var resourceName = project.Get(resourceField, resourceId);
                var units = project.Get(AssignmentFields.Units, a);

                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(resourceName);

                if (units != 1.0)
                {
                    sb.Append(" [");
                    sb.Append(Math.Round(units * 100, 2, MidpointRounding.AwayFromZero));
                    sb.Append("%]");
                }
            }

            var resourceNames = sb.ToString();
            return project.SetRaw(taskField, id, resourceNames);
        }

        private static ProjectData SetTaskResourceNamesOrInitials(ProjectData project, TaskId id, string value, bool isInitials)
        {
            var field = isInitials ? ResourceFields.Initials : ResourceFields.Name;

            value = value.Trim();

            var remainingAssignmentIds = project.GetAssignments(id).ToList();

            if (value.Length > 0)
            {
                var resourceParts = value.Split(',');

                foreach (var resourcePart in resourceParts)
                {
                    var initials = resourcePart.Trim();
                    var units = 1.0;
                    var openBracket = initials.IndexOf("[");
                    if (openBracket >= 0)
                    {
                        var closeBracket = initials.IndexOf("]");

                        if (closeBracket < openBracket)
                            throw new FormatException("Missing ']'");

                        var percentageText = initials.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        if (percentageText.EndsWith("%"))
                            percentageText = percentageText.Substring(0, percentageText.Length - 1).Trim();

                        if (!double.TryParse(percentageText, out var percentage))
                            throw new FormatException($"'{percentageText}' isn't a valid percentage");

                        initials = initials.Substring(0, openBracket).Trim();
                        units = percentage / 100.0;
                    }

                    var resourceId = project.GetResources(initials, isInitials).FirstOrDefault();
                    if (resourceId.IsDefault)
                    {
                        resourceId = ResourceId.Create();
                        project = project.AddResource(resourceId).Set(field, resourceId, initials);
                    }

                    var assignmentId = project.GetAssignment(id, resourceId);
                    if (assignmentId.IsDefault)
                    {
                        assignmentId = AssignmentId.Create();
                        project = project.AddAssignment(assignmentId, id, resourceId);
                    }

                    project = project.Set(AssignmentFields.Units, assignmentId, units);
                    remainingAssignmentIds.Remove(assignmentId);
                }
            }

            foreach (var assignmentId in remainingAssignmentIds)
                project = project.RemoveAssignment(assignmentId);

            return project;
        }
    }
}
