using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
                var predecessors = project.Get(TaskFields.PredecessorLinks, taskId);
                var allPredecessorsComputed = predecessors.All(l => computedTasks.Contains(l.PredecessorId));
                if (!allPredecessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    ComputeEarlyStartAndFinish(project, taskId, predecessors, out var earlyStart, out var earlyFinish);

                    project = project.SetRaw(TaskFields.EarlyStart, taskId, earlyStart)
                                     .SetRaw(TaskFields.EarlyFinish, taskId, earlyFinish);

                    computedTasks.Add(taskId);
                }
            }

            var projectFinish = ComputeProjectFinish(project);
            var information = project.Information.WithFinish(projectFinish);
            return project.WithInformation(information);
        }

        private static ProjectData BackwardPass(this ProjectData project)
        {
            var computedTasks = new HashSet<TaskId>();
            var toBeScheduled = new Queue<TaskId>(project.Tasks);

            while (toBeScheduled.Count > 0)
            {
                var taskId = toBeScheduled.Dequeue();
                var successors = project.Get(TaskFields.SuccessorLinks, taskId);
                var allSuccessorsComputed = successors.All(l => computedTasks.Contains(l.SuccessorId));
                if (!allSuccessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    ComputeLateStartAndFinish(project, taskId, successors, out var lateStart, out var lateFinish);

                    project = project.SetRaw(TaskFields.LateStart, taskId, lateStart)
                                     .SetRaw(TaskFields.LateFinish, taskId, lateFinish);

                    computedTasks.Add(taskId);
                }
            }

            return project;
        }

        private static ProjectData Finalize(this ProjectData project)
        {
            var calendar = project.Information.Calendar;

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

                var freeSlack = GetFreeSlack(project, taskId);
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

        private static TimeSpan GetFreeSlack(ProjectData project, TaskId taskId)
        {
            var successors = project.Get(TaskFields.SuccessorLinks, taskId);

            if (successors.Any())
            {
                return successors.Select(l => GetFreeSlack(project, l)).Min();
            }
            else
            {
                var calendar = project.Information.Calendar;
                var earlyFinish = project.Get(TaskFields.EarlyFinish, taskId);
                var projectFinish = project.Information.Finish;
                return calendar.GetWork(earlyFinish, projectFinish);
            }
        }

        private static TimeSpan GetFreeSlack(ProjectData project, TaskLink link)
        {
            var calendar = project.Information.Calendar;

            var projectStart = project.Information.Start;
            var predecessorEarlyStart = project.Get(TaskFields.EarlyStart, link.PredecessorId);
            var predecessorEarlyFinish = project.Get(TaskFields.EarlyFinish, link.PredecessorId);
            var successorEarlyStart = project.Get(TaskFields.EarlyStart, link.SuccessorId);
            var successorEarlyFinish = project.Get(TaskFields.EarlyFinish, link.SuccessorId);

            switch (link.Type)
            {
                case TaskLinkType.FinishToStart:
                    return calendar.GetWork(predecessorEarlyFinish, successorEarlyStart) - link.Lag;
                case TaskLinkType.StartToStart:
                    return calendar.GetWork(predecessorEarlyStart, successorEarlyStart) - link.Lag;
                case TaskLinkType.FinishToFinish:
                {
                    // Finish-to-Finish is special in that it won't schedule tasks before the project's
                    // start date. Thus, we'll ignore the lag if the successor was supposed to end before
                    // us (i.e. had a negative slack) but ended up ending after ours (i.e. it couldn't
                    // start any earlier).

                    var successorCannotStartAnyEarlier = successorEarlyStart == projectStart &&
                                                         successorEarlyFinish > predecessorEarlyFinish &&
                                                         link.Lag < TimeSpan.Zero;
                    if (successorCannotStartAnyEarlier)
                        return calendar.GetWork(predecessorEarlyFinish, successorEarlyFinish);

                    return calendar.GetWork(predecessorEarlyFinish, successorEarlyFinish) - link.Lag;
                }
                case TaskLinkType.StartToFinish:
                    return calendar.GetWork(predecessorEarlyStart, successorEarlyFinish) - link.Lag;
                default:
                    throw new Exception($"Unexpected case label {link.Type}");
            }
        }

        private static TimeSpan GetWork(ProjectData project, TaskId taskId)
        {
            var hasAssignments = project.GetAssignments(taskId).Any();
            if (!hasAssignments)
                return project.Get(TaskFields.Duration, taskId);

            return project.Get(TaskFields.Work, taskId);
        }

        private static DateTimeOffset ComputeProjectFinish(ProjectData project)
        {
            return project.Tasks
                          .Select(t => project.Get(TaskFields.EarlyFinish, t))
                          .DefaultIfEmpty(project.Information.Start)
                          .Max();
        }

        private static void ComputeEarlyStartAndFinish(ProjectData project, TaskId taskId, ImmutableArray<TaskLink> predecessors, out DateTimeOffset earlyStart, out DateTimeOffset earlyFinish)
        {
            if (!predecessors.Any())
            {
                earlyStart = project.Information.Start;
                ComputeFinish(project, taskId, ref earlyStart, out earlyFinish);
            }
            else
            {
                earlyStart = DateTimeOffset.MinValue;
                earlyFinish = DateTimeOffset.MinValue;

                foreach (var predecessor in predecessors)
                {
                    ComputeEarlyStartAndFinish(project, taskId, predecessor, out var start, out var finish);
                    if (earlyStart <= start)
                    {
                        earlyStart = start;

                        if (earlyFinish <= finish)
                            earlyFinish = finish;
                    }
                }
            }
        }

        private static void ComputeEarlyStartAndFinish(ProjectData project, TaskId taskId, TaskLink predecessorLink, out DateTimeOffset earlyStart, out DateTimeOffset earlyFinish)
        {
            var calendar = project.Information.Calendar;
            var type = predecessorLink.Type;
            var predecessorEarlyStart = project.Get(TaskFields.EarlyStart, predecessorLink.PredecessorId);
            var predecessorEarlyFinish = project.Get(TaskFields.EarlyFinish, predecessorLink.PredecessorId);

            switch (type)
            {
                case TaskLinkType.FinishToStart:
                    earlyStart = predecessorEarlyFinish;
                    break;
                case TaskLinkType.StartToStart:
                    earlyStart = predecessorEarlyStart;
                    break;
                case TaskLinkType.FinishToFinish:
                    earlyFinish = predecessorEarlyFinish;
                    ComputeStart(project, taskId, out earlyStart, ref earlyFinish);
                    break;
                case TaskLinkType.StartToFinish:
                    earlyFinish = predecessorEarlyStart;
                    ComputeStart(project, taskId, out earlyStart, ref earlyFinish);
                    break;
                default:
                    throw new Exception($"Unexpected case label {type}");
            }

            if (predecessorLink.Lag != TimeSpan.Zero)
                earlyStart = project.Information.Calendar.AddWork(earlyStart, predecessorLink.Lag);

            var mustSnap = type == TaskLinkType.FinishToFinish;
            if (mustSnap && earlyStart < project.Information.Start)
                earlyStart = project.Information.Start;

            ComputeFinish(project, taskId, ref earlyStart, out earlyFinish);

            // In case of Start-to-Finish, the end date is snapped to a start time
            // unless there is positive lag.

            if (type == TaskLinkType.StartToFinish && predecessorLink.Lag <= TimeSpan.Zero)
                earlyFinish = calendar.FindWorkStart(earlyFinish);
        }

        private static void ComputeLateStartAndFinish(ProjectData project, TaskId taskId, ImmutableArray<TaskLink> successors, out DateTimeOffset lateStart, out DateTimeOffset lateFinish)
        {
            if (!successors.Any())
            {
                lateFinish = project.Information.Finish;
                ComputeStart(project, taskId, out lateStart, ref lateFinish);
            }
            else
            {
                lateStart = DateTimeOffset.MaxValue;
                lateFinish = DateTimeOffset.MaxValue;

                foreach (var successor in successors)
                {
                    ComputeLateStartAndFinish(project, taskId, successor, out var start, out var finish);

                    if (lateFinish >= finish)
                    {
                        lateFinish = finish;

                        if (lateStart >= start)
                            lateStart = start;
                    }
                }
            }
        }

        private static void ComputeLateStartAndFinish(ProjectData project, TaskId taskId, TaskLink successorLink, out DateTimeOffset lateStart, out DateTimeOffset lateFinish)
        {
            var calendar = project.Information.Calendar;

            switch (successorLink.Type)
            {
                case TaskLinkType.FinishToStart:
                    lateFinish = project.Get(TaskFields.LateStart, successorLink.SuccessorId);
                    break;
                case TaskLinkType.StartToStart:
                    lateStart = project.Get(TaskFields.LateStart, successorLink.SuccessorId);
                    ComputeFinish(project, taskId, ref lateStart, out lateFinish);
                    break;
                case TaskLinkType.FinishToFinish:
                    lateFinish = project.Get(TaskFields.LateFinish, successorLink.SuccessorId);
                    break;
                case TaskLinkType.StartToFinish:
                    lateStart = project.Get(TaskFields.LateFinish, successorLink.SuccessorId);
                    ComputeFinish(project, taskId, ref lateStart, out lateFinish);
                    break;
                default:
                    throw new Exception($"Unexpected case label {successorLink.Type}");
            }

            if (successorLink.Lag != TimeSpan.Zero)
                lateFinish = project.Information.Calendar.AddWork(lateFinish, -successorLink.Lag);

            // TODO: Is this correct? We probably need to align with with ComputeEarlyStart.
            if (lateFinish > project.Information.Finish)
                lateFinish = project.Information.Finish;

            ComputeStart(project, taskId, out lateStart, ref lateFinish);
        }

        private static void ComputeFinish(ProjectData project, TaskId taskId, ref DateTimeOffset start, out DateTimeOffset finish)
        {
            var assignmentIds = project.GetAssignments(taskId);
            if (!assignmentIds.Any())
            {
                var work = GetWork(project, taskId);
                ComputeFinish(project.Information.Calendar, ref start, out finish, work);
            }
            else
            {
                finish = DateTimeOffset.MinValue;

                foreach (var assignmentId in assignmentIds)
                {
                    var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                    var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                    var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                    ComputeFinish(project.Information.Calendar, ref start, out var assignmentFinish, duration);

                    if (assignmentFinish > finish)
                        finish = assignmentFinish;
                }
            }
        }

        private static void ComputeStart(ProjectData project, TaskId taskId, out DateTimeOffset start, ref DateTimeOffset finish)
        {
            var assignmentIds = project.GetAssignments(taskId);
            if (!assignmentIds.Any())
            {
                var work = GetWork(project, taskId);
                ComputeStart(project.Information.Calendar, out start, ref finish, work);

            }
            else
            {
                start = DateTimeOffset.MaxValue;

                foreach (var assignmentId in assignmentIds)
                {
                    var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                    var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                    var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                    ComputeStart(project.Information.Calendar, out var assignmentStart, ref finish, duration);

                    if (assignmentStart < start)
                        start = assignmentStart;
                }
            }
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
