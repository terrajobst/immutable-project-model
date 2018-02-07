using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public Task AddTask(TaskId taskId = default)
        {
            taskId = taskId.CreateIfDefault();

            if (GetTask(taskId) != null)
                throw new ArgumentException($"Project already contains a task with ID {taskId}.");

            var taskData = TaskData.Create(taskId).SetValue(TaskFields.Ordinal, Data.Tasks.Count);
            var projectData = Data.AddTask(taskData);
            return UpdateProject(projectData).GetTask(taskData.Id);
        }

        public Project RemoveTask(TaskId taskId)
        {
            var project = Data.RemoveTask(taskId);

            // Update tasks

            var newTasks = project.Tasks;

            var ordinal = 0;

            foreach (var task in project.Tasks.Values.OrderBy(t => t.Ordinal))
            {
                var newTask = task;

                // Update Ordinal
                newTask = newTask.SetValue(TaskFields.Ordinal, ordinal);
                ordinal++;

                // Update PredecessorIds
                var predecessors = newTask.PredecessorIds.Remove(taskId);
                newTask = newTask.SetValue(TaskFields.PredecessorIds, predecessors);

                newTasks = newTasks.SetItem(newTask.Id, newTask);
            }

            project = project.WithTasks(newTasks);

            // Update assignments

            var taskAssignments = project.Assignments.Values.Where(a => a.TaskId == taskId)
                                                            .Select(a => a.Id);
            var newAssignments = project.Assignments.RemoveRange(taskAssignments);

            project = project.WithAssignments(newAssignments);

            return UpdateProject(project);
        }

        internal Task SetTaskField(Task task, TaskField field, object value)
        {
            ProjectData project;

            if (field == TaskFields.Ordinal)
            {
                project = SetTaskOrdinal(Data, task.Id, (int)value);
            }
            else if (field == TaskFields.Name)
            {
                project = SetTaskName(Data, task.Id, (string)value);
            }
            else if (field == TaskFields.Duration)
            {
                project = SetTaskDuration(Data, task.Id, (TimeSpan)value);
            }
            else if (field == TaskFields.Work)
            {
                project = SetTaskWork(Data, task.Id, (TimeSpan)value);
            }
            else if (field == TaskFields.PredecessorIds)
            {
                project = SetTaskPredecessorIds(Data, task.Id, (ImmutableArray<TaskId>)value);
            }
            else if (field == TaskFields.ResourceNames)
            {
                project = SetTaskResourceNames(Data, task.Id, (string)value);
            }
            else if (field == TaskFields.Predecessors)
            {
                project = SetTaskPredecessors(Data, task.Id, (string)value);
            }
            else
            {
                var taskData = task.Data.SetValue(field, value);
                project = Data.UpdateTask(taskData);
            }

            return UpdateProject(project).GetTask(task.Id);
        }

        private ProjectData SetTaskOrdinal(ProjectData project, TaskId id, int value)
        {
            if (value < 0 || value >= project.Tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            var oldOrdinal = project.Tasks[id].Ordinal;
            var newOrdinal = value;

            var orderedTasks = project.Tasks.Values.OrderBy(t => t.Ordinal).ToList();
            var task = orderedTasks[oldOrdinal];
            orderedTasks.RemoveAt(oldOrdinal);
            orderedTasks.Insert(newOrdinal, task);

            var tasks = project.Tasks;

            for (var i = 0; i < orderedTasks.Count; i++)
            {
                var t = orderedTasks[i].SetValue(TaskFields.Ordinal, i);
                tasks = tasks.SetItem(t.Id, t);
            }

            return project.WithTasks(tasks);
        }

        private ProjectData SetTaskName(ProjectData project, TaskId id, string value)
        {
            project = project.UpdateTask(project.Tasks[id].SetValue(TaskFields.Name, value));

            var newAssignments = project.Assignments;

            foreach (var assignment in project.Assignments.Values.Where(a => a.TaskId == id))
                newAssignments = newAssignments.SetItem(assignment.Id, assignment.SetValue(AssignmentFields.TaskName, value));

            return project.WithAssignments(newAssignments);
        }

        private ProjectData SetTaskDuration(ProjectData project, TaskId id, TimeSpan value)
        {
            return Scheduler.SetTaskDuration(project, project.Tasks[id], value);
        }

        private ProjectData SetTaskWork(ProjectData project, TaskId id, TimeSpan value)
        {
            return Scheduler.SetTaskWork(project, project.Tasks[id], value);
        }

        private ProjectData SetTaskPredecessorIds(ProjectData project, TaskId id, ImmutableArray<TaskId> value)
        {
            // Check for cycles

            var queue = new Queue<TaskId>();

            foreach (var predecessorId in value)
            {
                queue.Enqueue(predecessorId);

                while (queue.Count > 0)
                {
                    var taskId = queue.Dequeue();
                    if (taskId == id)
                    {
                        var predecessorOrdinal = project.Tasks[predecessorId].Ordinal;
                        throw new InvalidOperationException($"Adding {predecessorOrdinal} as a predecessor would cause a cycle");
                    }

                    var task = project.Tasks[taskId];
                    foreach (var pid in task.PredecessorIds)
                        queue.Enqueue(pid);
                }
            }

            project = project.UpdateTask(project.Tasks[id].SetValue(TaskFields.PredecessorIds, value));
            project = InitializeTaskPredecessors(project, id);
            return project;
        }

        private ProjectData InitializeTaskResourceNames(ProjectData project, TaskId id)
        {
            var sb = new StringBuilder();

            foreach (var a in project.Assignments.Values.Where(a => a.TaskId == id)
                                                        .OrderBy(a => a.ResourceName))
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                sb.Append(a.ResourceName);

                if (a.Units != 1.0)
                {
                    sb.Append(" [");
                    sb.Append(Math.Round(a.Units * 100, 2, MidpointRounding.AwayFromZero));
                    sb.Append("%]");
                }
            }

            var resourceNames = sb.ToString();
            return project.UpdateTask(project.Tasks[id].SetValue(TaskFields.ResourceNames, resourceNames));
        }

        private ProjectData SetTaskResourceNames(ProjectData project, TaskId id, string value)
        {
            var result = this;

            value = value.Trim();

            var remainingAssignmentIds = result.Assignments.Where(a => a.Task.Id == id)
                                                           .Select(a => a.Id)
                                                           .ToList();

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

                    var resource = result.Resources.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
                    if (resource == null)
                    {
                        resource = result.AddResource().WithName(name);
                        result = resource.Project;
                    }

                    var assignment = result.Assignments.FirstOrDefault(a => a.Task.Id == id && a.Resource.Id == resource.Id);
                    if (assignment == null)
                    {
                        assignment = result.AddAssignment(id, resource.Id);
                        result = assignment.Project;
                    }

                    result = assignment.WithUnits(units).Project;
                    remainingAssignmentIds.Remove(assignment.Id);
                }
            }

            foreach (var assignmentId in remainingAssignmentIds)
                result = result.RemoveAssignment(assignmentId);

            return result.Data;
        }

        private ProjectData InitializeTaskPredecessors(ProjectData project, TaskId id)
        {
            var sb = new StringBuilder();

            foreach (var t in project.Tasks[id].PredecessorIds.Select(pid => project.Tasks[pid]).OrderBy(t => t.Ordinal))
            {
                if (sb.Length > 0)
                    sb.Append(",");

                sb.Append(t.Ordinal);
            }

            var precedessors = sb.ToString();
            return project.UpdateTask(project.Tasks[id].SetValue(TaskFields.Predecessors, precedessors));
        }

        private ProjectData SetTaskPredecessors(ProjectData project, TaskId id, string value)
        {
            var task = GetTask(id);

            value = value.Trim();

            var remainingPredecessorIds = task.Project.GetTask(id).PredecessorIds.ToList();

            if (value.Length > 0)
            {
                var predecessorIdParts = value.Split(',');

                foreach (var predecessorIdPart in predecessorIdParts)
                {
                    var predecessorIdText = predecessorIdPart.Trim();

                    if (!int.TryParse(predecessorIdText, out var predecessorId))
                        throw new FormatException($"'{predecessorIdText}' isn't a valid int");

                    var predecessor = task.Project.Tasks.SingleOrDefault(t => t.Ordinal == predecessorId);
                    if (predecessor == null)
                        throw new FormatException($"'{predecessorId}' isn't a valid predecessor");

                    task = task.AddPredecessorId(predecessor.Id);

                    remainingPredecessorIds.Remove(predecessor.Id);
                }
            }

            foreach (var predecessorIds in remainingPredecessorIds)
                task = task.RemovePredecessorId(predecessorIds);

            return task.Project.Data;
        }
    }
}
