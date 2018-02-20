using System;
using System.Collections.Generic;
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
            var toBeScheduled = new Queue<TaskId>(project.Tasks);

            while (toBeScheduled.Count > 0)
            {
                var taskId = toBeScheduled.Dequeue();
                var predecessorIds = project.GetPredecessors(taskId);
                var allPredecessorsComputed = predecessorIds.All(id => computedTasks.Contains(id));
                if (!allPredecessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    var earlyStart = predecessorIds.Select(p => project.Get(TaskFields.EarlyFinish, p))
                                                   .DefaultIfEmpty(project.Information.StartDate)
                                                   .Max();

                    var assignmentIds = project.GetAssignments(taskId);
                    if (!assignmentIds.Any())
                    {
                        var work = GetWork(project, taskId);
                        ComputeFinish(project.Information.Calendar, ref earlyStart, out var earlyFinish, work);

                        project = project.SetRaw(TaskFields.EarlyStart, taskId, earlyStart)
                                         .SetRaw(TaskFields.EarlyFinish, taskId, earlyFinish);
                    }
                    else
                    {
                        var earlyFinish = DateTimeOffset.MinValue;

                        foreach (var assignmentId in assignmentIds)
                        {
                            var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                            var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                            var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                            ComputeFinish(project.Information.Calendar, ref earlyStart, out var assignmentFinish, duration);

                            if (assignmentFinish > earlyFinish)
                                earlyFinish = assignmentFinish;
                        }

                        project = project.SetRaw(TaskFields.EarlyStart, taskId, earlyStart)
                                         .SetRaw(TaskFields.EarlyFinish, taskId, earlyFinish);
                    }

                    computedTasks.Add(taskId);
                }
            }

            return project;
        }

        private static ProjectData BackwardPass(this ProjectData project)
        {
            var projectEnd = project.Tasks
                                    .Select(t => project.Get(TaskFields.EarlyFinish, t))
                                    .DefaultIfEmpty()
                                    .Max();

            var computedTasks = new HashSet<TaskId>();
            var toBeScheduled = new Queue<TaskId>(project.Tasks);

            while (toBeScheduled.Count > 0)
            {
                var taskId = toBeScheduled.Dequeue();
                var successors = project.GetSuccessors(taskId);
                var allSuccessorsComputed = successors.All(id => computedTasks.Contains(id));
                if (!allSuccessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    var lateFinish = successors.Select(id => project.Get(TaskFields.LateStart, id))
                                               .DefaultIfEmpty(projectEnd)
                                               .Min();

                    var assignmentIds = project.GetAssignments(taskId);
                    if (!assignmentIds.Any())
                    {
                        var work = GetWork(project, taskId);
                        ComputeStart(project.Information.Calendar, out var lateStart, ref lateFinish, work);

                        project = project.SetRaw(TaskFields.LateStart, taskId, lateStart)
                                         .SetRaw(TaskFields.LateFinish, taskId, lateFinish);
                    }
                    else
                    {
                        var lateStart = DateTimeOffset.MaxValue;

                        foreach (var assignmentId in assignmentIds)
                        {
                            var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                            var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                            var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                            ComputeStart(project.Information.Calendar, out var assignmentStart, ref lateFinish, duration);

                            if (assignmentStart < lateStart)
                                lateStart = assignmentStart;
                        }

                        project = project.SetRaw(TaskFields.LateStart, taskId, lateStart)
                                         .SetRaw(TaskFields.LateFinish, taskId, lateFinish);
                    }

                    computedTasks.Add(taskId);
                }
            }

            return project;
        }

        private static ProjectData Finalize(this ProjectData project)
        {
            var calendar = project.Information.Calendar;

            var projectEnd = project.Tasks                                    
                                    .Select(t => project.Get(TaskFields.EarlyFinish, t))
                                    .DefaultIfEmpty()
                                    .Max();

            var successorsById = project.Tasks                                        
                                        .SelectMany(s => project.GetPredecessors(s), (s, p) => (predecessor: p, successor: s))
                                        .ToLookup(t => t.predecessor, t => t.successor);

            foreach (var taskId in project.Tasks)
            {
                var earlyStart = project.Get(TaskFields.EarlyStart, taskId);
                var earlyFinish = project.Get(TaskFields.EarlyFinish, taskId);
                var lateStart = project.Get(TaskFields.LateStart, taskId);
                var lateFinish = project.Get(TaskFields.LateFinish, taskId);

                // Set start, finish, and duration

                var start = earlyStart;
                var finish = earlyFinish;
                var duration = calendar.GetWork(start, finish);

                project = project.SetRaw(TaskFields.Start, taskId, start)
                                 .SetRaw(TaskFields.Finish, taskId, finish)
                                 .SetRaw(TaskFields.Duration, taskId, duration);

                // Set start slack, finish slack, total slack, and criticality

                var startSlack = calendar.GetWork(earlyStart, lateStart);
                var finishSlack = calendar.GetWork(earlyFinish, lateFinish);
                var totalSlack = startSlack <= finishSlack ? startSlack : finishSlack;
                var isCritical = totalSlack == TimeSpan.Zero;

                project = project.SetRaw(TaskFields.StartSlack, taskId, startSlack)
                                 .SetRaw(TaskFields.FinishSlack, taskId, finishSlack)
                                 .SetRaw(TaskFields.TotalSlack, taskId, totalSlack)
                                 .SetRaw(TaskFields.IsCritical, taskId, isCritical);

                // Set free slack

                var minumumEarlyStartOfSuccessors = successorsById[taskId].Select(t => project.Get(TaskFields.EarlyStart, t))
                                                                          .DefaultIfEmpty(projectEnd)
                                                                          .Min();
                var freeSlack = calendar.GetWork(earlyStart, minumumEarlyStartOfSuccessors) - duration;
                project = project.SetRaw(TaskFields.FreeSlack, taskId, freeSlack);
            }

            // Set assignment start and finish

            foreach (var assignmentId in project.Assignments)
            {
                var taskId = project.Get(AssignmentFields.TaskId, assignmentId);
                var taskStart = project.Get(TaskFields.Start, taskId);

                var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);
                var assignmentStart = taskStart;

                var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                ComputeFinish(project.Information.Calendar, ref assignmentStart, out var assignmentFinish, duration);

                project = project.SetRaw(AssignmentFields.Start, assignmentId, assignmentStart)
                                 .SetRaw(AssignmentFields.Finish, assignmentId, assignmentFinish);
            }

            return project;
        }

        private static TimeSpan GetWork(ProjectData project, TaskId taskId)
        {
            var hasAssignments = project.GetAssignments(taskId).Any();
            if (!hasAssignments)
                return project.Get(TaskFields.Duration, taskId);

            return project.Get(TaskFields.Work, taskId);
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
    }
}
