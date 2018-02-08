using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal static class Scheduler
    {
        public static ProjectData Schedule(ProjectData project)
        {
            return project.ForwardPass()
                          .BackwardPass()
                          .Finalize();
        }

        private static ProjectData ForwardPass(this ProjectData project)
        {
            var computedTasks = new HashSet<TaskId>();
            var toBeScheduled = new Queue<TaskId>(project.Tasks.Keys);

            while (toBeScheduled.Count > 0)
            {
                var taskId = toBeScheduled.Dequeue();
                var task = project.Tasks[taskId];
                var allPredecessorsComputed = task.PredecessorIds.All(id => computedTasks.Contains(id));
                if (!allPredecessorsComputed)
                {
                    toBeScheduled.Enqueue(task.Id);
                }
                else
                {
                    var predecessors = task.PredecessorIds.Select(id => project.Tasks[id]);

                    var earlyStart = predecessors.Select(p => p.EarlyFinish)
                                                 .DefaultIfEmpty(project.Information.StartDate)
                                                 .Max();

                    var assignments = project.Assignments.Values.Where(a => a.TaskId == taskId);
                    if (!assignments.Any())
                    {
                        var work = GetWork(project, task);
                        ComputeFinish(project.Information.Calendar, ref earlyStart, out var earlyFinish, work);

                        task = task.SetValue(TaskFields.EarlyStart, earlyStart)
                                   .SetValue(TaskFields.EarlyFinish, earlyFinish);
                        project = project.UpdateTask(task);
                    }
                    else
                    {
                        var earlyFinish = DateTimeOffset.MinValue;

                        foreach (var assignment in assignments)
                        {
                            var time = TimeSpan.FromHours(assignment.Work.TotalHours * (1.0 / assignment.Units));
                            ComputeFinish(project.Information.Calendar, ref earlyStart, out var assignmentFinish, time);

                            if (assignmentFinish > earlyFinish)
                                earlyFinish = assignmentFinish;
                        }

                        task = task.SetValue(TaskFields.EarlyStart, earlyStart)
                                   .SetValue(TaskFields.EarlyFinish, earlyFinish);
                        project = project.UpdateTask(task);
                    }

                    computedTasks.Add(taskId);
                }
            }

            return project;
        }

        private static ProjectData BackwardPass(this ProjectData project)
        {
            var projectEnd = project.Tasks.Values.Select(t => t.EarlyFinish)
                                                 .DefaultIfEmpty()
                                                 .Max();

            var computedTasks = new HashSet<TaskId>();
            var successorsById = project.Tasks
                                            .Values
                                            .SelectMany(s => s.PredecessorIds, (s, pId) => (predecessor: pId, successor: s.Id))
                                            .ToLookup(t => t.predecessor, t => t.successor);

            var toBeScheduled = new Queue<TaskId>(project.Tasks.Keys);

            while (toBeScheduled.Count > 0)
            {
                var taskId = toBeScheduled.Dequeue();
                var task = project.Tasks[taskId];
                var successors = successorsById[task.Id];
                var allSuccessorsComputed = successors.All(id => computedTasks.Contains(id));
                if (!allSuccessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    var lateFinish = successors.Select(id => project.Tasks[id].LateStart)
                                               .DefaultIfEmpty(projectEnd)
                                               .Min();

                    var assignments = project.Assignments.Values.Where(a => a.TaskId == taskId);
                    if (!assignments.Any())
                    {
                        var work = GetWork(project, task);
                        ComputeStart(project.Information.Calendar, out var lateStart, ref lateFinish, work);

                        task = task.SetValue(TaskFields.LateStart, lateStart)
                                   .SetValue(TaskFields.LateFinish, lateFinish);
                        project = project.UpdateTask(task);
                    }
                    else
                    {
                        var lateStart = DateTimeOffset.MaxValue;

                        foreach (var assignment in assignments)
                        {
                            var time = TimeSpan.FromHours(assignment.Work.TotalHours * (1.0 / assignment.Units));
                            ComputeStart(project.Information.Calendar, out var assignmentStart, ref lateFinish, time);

                            if (assignmentStart < lateStart)
                                lateStart = assignmentStart;
                        }

                        task = task.SetValue(TaskFields.LateStart, lateStart)
                                   .SetValue(TaskFields.LateFinish, lateFinish);
                        project = project.UpdateTask(task);
                    }

                    computedTasks.Add(task.Id);
                }
            }

            return project;
        }

        private static ProjectData Finalize(this ProjectData project)
        {
            var calendar = project.Information.Calendar;

            var projectEnd = project.Tasks.Values.Select(t => t.EarlyFinish)
                                    .DefaultIfEmpty()
                                    .Max();

            var successorsById = project.Tasks
                                .Values
                                .SelectMany(s => s.PredecessorIds, (s, pId) => (predecessor: pId, successor: s.Id))
                                .ToLookup(t => t.predecessor, t => t.successor);

            foreach (var task in project.Tasks.Values)
            {
                // Set Start & finish
                var newTask = task.SetValue(TaskFields.Start, task.EarlyStart)
                                  .SetValue(TaskFields.Finish, task.EarlyFinish);

                // Set duration
                var work = calendar.GetWork(newTask.Start, newTask.Finish);
                newTask = newTask.SetValue(TaskFields.Duration, work);

                project = project.UpdateTask(newTask);
            }

            foreach (var task in project.Tasks.Values)
            {
                var newTask = task;

                // Set start slack, finish slack, and start slack
                var startSlack = calendar.GetWork(task.EarlyStart, task.LateStart);
                var finishSlack = calendar.GetWork(task.EarlyFinish, task.LateFinish);
                var totalSlack = startSlack <= finishSlack ? startSlack : finishSlack;
                newTask = newTask.SetValue(TaskFields.StartSlack, startSlack)
                                 .SetValue(TaskFields.FinishSlack, finishSlack)
                                 .SetValue(TaskFields.TotalSlack, totalSlack);

                // Set free slack
                var successors = successorsById[newTask.Id].Select(id => project.Tasks[id]);
                var minumumEarlyStartOfSuccessors = successors.Select(t => t.EarlyStart)
                                                              .DefaultIfEmpty(projectEnd)
                                                              .Min();
                var freeSlack = calendar.GetWork(newTask.EarlyStart, minumumEarlyStartOfSuccessors) - newTask.Duration;
                newTask = newTask.SetValue(TaskFields.FreeSlack, freeSlack);

                // Set criticality
                var isCritical = newTask.TotalSlack == TimeSpan.Zero;
                newTask = newTask.SetValue(TaskFields.IsCritical, isCritical);

                project = project.UpdateTask(newTask);
            }

            var assignments = project.Assignments.Values;

            foreach (var assignment in assignments)
            {
                var task = project.Tasks[assignment.TaskId];
                var assignmentStart = task.Start;

                var time = TimeSpan.FromHours(assignment.Work.TotalHours * (1.0 / assignment.Units));
                ComputeFinish(project.Information.Calendar, ref assignmentStart, out var assignmentFinish, time);

                var newAssignment = assignment.SetValue(AssignmentFields.Start, assignmentStart)
                                              .SetValue(AssignmentFields.Finish, assignmentFinish);

                project = project.UpdateAssignment(newAssignment);
            }

            return project;
        }

        private static TimeSpan GetWork(ProjectData project, TaskData task)
        {
            var hasAssignments = project.Assignments.Values.Any(a => a.TaskId == task.Id);
            if (!hasAssignments)
                return task.Duration;

            return task.Work;
        }

        private static void ComputeFinish(Calendar calendar, ref DateTimeOffset start, out DateTimeOffset end, TimeSpan work)
        {
            if (work == TimeSpan.Zero)
            {
                end = start;
                return;
            }

            start = calendar.FindWorkStart(start);
            end = calendar.AddWork(start, work);
        }

        private static void ComputeStart(Calendar calendar, out DateTimeOffset start, ref DateTimeOffset end, TimeSpan work)
        {
            if (work == TimeSpan.Zero)
            {
                start = end;
                return;
            }

            end = calendar.FindWorkEnd(end);
            start = calendar.AddWork(end, -work);
        }

        public static ProjectData SetTaskWork(ProjectData project, TaskData task, TimeSpan work)
        {
            task = task.SetValue(TaskFields.Work, work);
            project = project.UpdateTask(task);

            var taskAssignments = project.Assignments.Values.Where(a => a.TaskId == task.Id).ToArray();

            if (work == TimeSpan.Zero)
            {
                for (var i = 0; i < taskAssignments.Length; i++)
                    taskAssignments[i] = taskAssignments[i].SetValue(AssignmentFields.Work, TimeSpan.Zero);
            }
            else
            {
                var totalExistingWork = TimeSpan.Zero;

                foreach (var a in taskAssignments)
                    totalExistingWork += a.Work;

                for (var i = 0; i < taskAssignments.Length; i++)
                {
                    double newHours;

                    if (totalExistingWork > TimeSpan.Zero)
                    {
                        newHours = taskAssignments[i].Work.TotalHours / totalExistingWork.TotalHours * work.TotalHours;
                    }
                    else
                    {
                        newHours = work.TotalHours / taskAssignments.Length;
                    }

                    var newWork = TimeSpan.FromHours(newHours);
                    taskAssignments[i] = taskAssignments[i].SetValue(AssignmentFields.Work, newWork);
                }
            }

            foreach (var assignment in taskAssignments)
                project = project.UpdateAssignment(assignment);

            return project;
        }

        public static ProjectData SetTaskDuration(ProjectData project, TaskData task, TimeSpan duration)
        {
            task = task.SetValue(TaskFields.Duration, duration);
            project = project.UpdateTask(task);

            var hasAssignments = project.Assignments.Values.Any(a => a.TaskId == task.Id);
            if (!hasAssignments)
                return project;

            foreach (var assignment in project.Assignments.Values.Where(a => a.TaskId == task.Id))
            {
                if (assignment.Finish == task.Finish)
                {
                    var assignmentWork = TimeSpan.FromHours(duration.TotalHours * assignment.Units);
                    project = SetAssignmentWork(project, assignment, assignmentWork);
                }
            }

            return project;
        }

        public static ProjectData AddAssignment(ProjectData project, AssignmentId assignmentId, TaskId taskId, ResourceId resourceId)
        {
            var assignment = AssignmentData.Create(assignmentId, taskId, resourceId);

            var task = project.Tasks[taskId];
            var allAssignments = project.Assignments.Values.Where(a => a.TaskId == taskId).ToArray();

            if (allAssignments.Length == 0)
            {
                var work = task.Work != TimeSpan.Zero
                            ? task.Work
                            : GetWork(project, task);

                task = task.SetValue(TaskFields.Work, work);
                project = project.UpdateTask(task);

                assignment = assignment.SetValue(AssignmentFields.Work, work);
                project = project.AddAssignment(assignment);
            }
            else
            {
                var newTaskWorkHours = task.Work.TotalHours / allAssignments.Length * (allAssignments.Length + 1);
                var newAssignmentWorkHours = newTaskWorkHours - task.Work.TotalHours;

                var newTaskWork = TimeSpan.FromHours(newTaskWorkHours);
                var newAssignmentWork = TimeSpan.FromHours(newAssignmentWorkHours);

                task = task.SetValue(TaskFields.Work, newTaskWork);
                project = project.UpdateTask(task);

                assignment = assignment.SetValue(AssignmentFields.Work, newAssignmentWork);
                project = project.AddAssignment(assignment);
            }

            return project;
        }

        public static ProjectData RemoveAssignment(ProjectData project, AssignmentId assignmentId)
        {
            if (!project.Assignments.TryGetValue(assignmentId, out var assignment))
                return project;

            project = project.RemoveAssignment(assignmentId);

            var task = project.Tasks[assignment.TaskId];
            project = SetTaskWork(project, task, task.Work - assignment.Work);

            return project;
        }

        public static ProjectData SetAssignmentWork(ProjectData project, AssignmentData assignment, TimeSpan work)
        {
            var task = project.Tasks[assignment.TaskId];
            var assignmentWorkDelta = work - assignment.Work;

            var taskWork = task.Work + assignmentWorkDelta;
            var newTask = task.SetValue(TaskFields.Work, taskWork);
            var newAssignment = assignment.SetValue(AssignmentFields.Work, work);

            return project.UpdateTask(newTask)
                          .UpdateAssignment(newAssignment);
        }
    }
}
