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
            else if (field == TaskFields.Start)
            {
                return SetTaskStart(this, id, (DateTimeOffset)value);
            }
            else if (field == TaskFields.Finish)
            {
                return SetTaskFinish(this, id, (DateTimeOffset)value);
            }
            else if (field == TaskFields.ConstraintType)
            {
                return SetTaskConstraintType(this, id, (ConstraintType)value);
            }
            else if (field == TaskFields.ConstraintDate)
            {
                return SetTaskConstraintDate(this, id, (DateTimeOffset?)value);
            }
            else if (field == TaskFields.Predecessors)
            {
                return SetTaskPredecessors(this, id, (string)value);
            }
            else if (field == TaskFields.ResourceNames)
            {
                return SetTaskResourceNames(this, id, (string)value);
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
            else if (field == TaskFields.ResourceNames)
            {
                return ResetTaskResourceNames(this, id);
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

                project = project.SetRaw(TaskFields.Ordinal, taskId, ordinal)
                                 .Reset(TaskFields.Predecessors, taskId);
            }

            // Then we can update all predecessors

            foreach (var taskId in orderedTaskIds)
                project = project.Reset(TaskFields.Predecessors, taskId);

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

        private static ProjectData SetTaskStart(ProjectData project, TaskId id, DateTimeOffset value)
        {
            return project.Set(TaskFields.ConstraintType, id, ConstraintType.StartNoEarlierThan)
                          .Set(TaskFields.ConstraintDate, id, value);
        }

        private static ProjectData SetTaskFinish(ProjectData project, TaskId id, DateTimeOffset value)
        {
            return project.Set(TaskFields.ConstraintType, id, ConstraintType.FinishNoEarlierThan)
                          .Set(TaskFields.ConstraintDate, id, value);
        }

        private static ProjectData SetTaskConstraintType(ProjectData project, TaskId id, ConstraintType value)
        {
            project = project.SetRaw(TaskFields.ConstraintType, id, value);

            var hasNoDate = value == ConstraintType.AsSoonAsPossible ||
                            value == ConstraintType.AsLateAsPossible;

            if (hasNoDate)
            {
                project = project.SetRaw(TaskFields.ConstraintDate, id, null);
            }
            else
            {
                var hasDate = project.Get(TaskFields.ConstraintDate, id) != null;
                if (!hasDate)
                {
                    var isStart = value == ConstraintType.MustStartOn ||
                                  value == ConstraintType.StartNoEarlierThan ||
                                  value == ConstraintType.StartNoLaterThan;
                    var date = isStart
                                ? project.Get(TaskFields.Start, id)
                                : project.Get(TaskFields.Finish, id);
                    project = project.SetRaw(TaskFields.ConstraintDate, id, date);
                }
            }

            return project;
        }

        private static ProjectData SetTaskConstraintDate(ProjectData project, TaskId id, DateTimeOffset? value)
        {
            var constraintType = project.Get(TaskFields.ConstraintType, id);
            var hasNoDate = constraintType == ConstraintType.AsSoonAsPossible ||
                            constraintType == ConstraintType.AsLateAsPossible;
            var requiresDate = !hasNoDate;

            if (hasNoDate && value != null)
                throw new InvalidOperationException($"Cannot set constraint date for a constraint of {constraintType}");
            else if (requiresDate && value == null)
                throw new InvalidOperationException($"Cannot clear constraint date for a constraint of {constraintType}");

            return project.SetRaw(TaskFields.ConstraintDate, id, value);
        }

        private static ProjectData ResetTaskPredecessors(ProjectData project, TaskId id)
        {
            var sb = new StringBuilder();

            foreach (var p in project.GetPredecessors(id).OrderBy(p => project.Get(TaskFields.Ordinal, p)))
            {
                if (sb.Length > 0)
                    sb.Append(",");

                var ordinal = project.Get(TaskFields.Ordinal, p);
                sb.Append(ordinal);
            }

            var precedessors = sb.ToString();
            return project.SetRaw(TaskFields.Predecessors, id, precedessors);
        }

        private static ProjectData SetTaskPredecessors(ProjectData project, TaskId id, string value)
        {
            value = value.Trim();

            var predecessorsBuilder = ImmutableArray.CreateBuilder<TaskId>();
            var remainingTaskLinks = project.Get(TaskFields.PredecessorLinks, id).ToList();

            if (value.Length > 0)
            {
                var predecessorOrdinalParts = value.Split(',');

                foreach (var predecessorOrdinalPart in predecessorOrdinalParts)
                {
                    var predecessorOrdinalText = predecessorOrdinalPart.Trim();

                    if (!int.TryParse(predecessorOrdinalText, out var predecessorOrdinal))
                        throw new FormatException($"'{predecessorOrdinalText}' isn't a valid int");

                    var predecessorId = project.GetTask(predecessorOrdinal);
                    if (predecessorId.IsDefault)
                        throw new FormatException($"'{predecessorOrdinal}' isn't a valid predecessor");

                    var taskLink = project.GetTaskLink(predecessorId, id);
                    if (taskLink == null)
                    {
                        taskLink = TaskLink.Create(predecessorId, id);
                        if (project.TaskLinkCausesCycle(taskLink))
                        {
                            var successorOrdinal = project.Get(TaskFields.Ordinal, taskLink.SuccessorId);
                            throw new InvalidOperationException($"Cannot add a link from task {predecessorOrdinal} to task {successorOrdinal} as this would cause a cycle.");
                        }

                        project = project.AddTaskLink(taskLink);
                    }

                    remainingTaskLinks.Remove(taskLink);
                }
            }

            foreach (var taskLink in remainingTaskLinks)
                project = project.RemoveTaskLink(taskLink);

            return project;
        }

        private static ProjectData ResetTaskResourceNames(ProjectData project, TaskId id)
        {
            var sb = new StringBuilder();

            foreach (var a in project.GetAssignments(id)
                                     .OrderBy(a => project.Get(AssignmentFields.ResourceName, a)))
            {
                var resourceName = project.Get(AssignmentFields.ResourceName, a);
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
            return project.SetRaw(TaskFields.ResourceNames, id, resourceNames);
        }

        private static ProjectData SetTaskResourceNames(ProjectData project, TaskId id, string value)
        {
            value = value.Trim();

            var remainingAssignmentIds = project.GetAssignments(id).ToList();

            if (value.Length > 0)
            {
                var resourceParts = value.Split(',');

                foreach (var resourcePart in resourceParts)
                {
                    var name = resourcePart.Trim();
                    var units = 1.0;
                    var openBracket = name.IndexOf("[");
                    if (openBracket >= 0)
                    {
                        var closeBracket = name.IndexOf("]");

                        if (closeBracket < openBracket)
                            throw new FormatException("Missing ']' in resource name");

                        var percentageText = name.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();

                        if (percentageText.EndsWith("%"))
                            percentageText = percentageText.Substring(0, percentageText.Length - 1).Trim();

                        if (!double.TryParse(percentageText, out var percentage))
                            throw new FormatException($"'{percentageText}' isn't a valid percentage");

                        name = name.Substring(0, openBracket).Trim();
                        units = percentage / 100.0;
                    }

                    var resourceId = project.GetResources(name).FirstOrDefault();
                    if (resourceId.IsDefault)
                    {
                        resourceId = ResourceId.Create();
                        project = project.AddResource(resourceId).Set(ResourceFields.Name, resourceId, name);
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
