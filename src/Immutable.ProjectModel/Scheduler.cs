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
                    var constraintType = project.Get(TaskFields.ConstraintType, taskId);
                    var constraintDate = project.Get(TaskFields.ConstraintDate, taskId);

                    var predecessorFinish = predecessorIds.Select(p => (DateTimeOffset?)project.Get(TaskFields.EarlyFinish, p))
                                                          .DefaultIfEmpty()
                                                          .Max();

                    var assignmentIds = project.GetAssignments(taskId);
                    if (!assignmentIds.Any())
                    {
                        var work = GetWork(project, taskId);
                        ComputeFinish(project.Information.Calendar,
                                      project.Information.Start,
                                      predecessorFinish,
                                      constraintType,
                                      constraintDate,
                                      out var earlyStart,
                                      out var earlyFinish,
                                      work);

                        project = project.SetRaw(TaskFields.EarlyStart, taskId, earlyStart)
                                         .SetRaw(TaskFields.EarlyFinish, taskId, earlyFinish);
                    }
                    else
                    {
                        var earlyStart = DateTimeOffset.MaxValue;
                        var earlyFinish = DateTimeOffset.MinValue;

                        foreach (var assignmentId in assignmentIds)
                        {
                            var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                            var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                            var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                            ComputeFinish(project.Information.Calendar,
                                          project.Information.Start,
                                          predecessorFinish,
                                          constraintType,
                                          constraintDate,
                                          out var assignmentStart,
                                          out var assignmentFinish,
                                          duration);

                            if (assignmentStart < earlyStart)
                                earlyStart = assignmentStart;

                            if (assignmentFinish > earlyFinish)
                                earlyFinish = assignmentFinish;
                        }

                        project = project.SetRaw(TaskFields.EarlyStart, taskId, earlyStart)
                                         .SetRaw(TaskFields.EarlyFinish, taskId, earlyFinish);
                    }

                    computedTasks.Add(taskId);
                }
            }

            var projectFinish = project.Tasks
                                       .Select(t => project.Get(TaskFields.EarlyFinish, t))
                                       .DefaultIfEmpty(project.Information.Start)
                                       .Max();

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
                var successors = project.GetSuccessors(taskId);
                var allSuccessorsComputed = successors.All(id => computedTasks.Contains(id));
                if (!allSuccessorsComputed)
                {
                    toBeScheduled.Enqueue(taskId);
                }
                else
                {
                    var constraintType = project.Get(TaskFields.ConstraintType, taskId);
                    var constraintDate = project.Get(TaskFields.ConstraintDate, taskId);

                    var successorStart = successors.Select(id => (DateTimeOffset?)project.Get(TaskFields.LateStart, id))
                                                   .DefaultIfEmpty()
                                                   .Min();

                    var assignmentIds = project.GetAssignments(taskId);
                    if (!assignmentIds.Any())
                    {
                        var work = GetWork(project, taskId);
                        ComputeStart(project.Information.Calendar,
                                     project.Information.Finish,
                                     successorStart,
                                     constraintType,
                                     constraintDate,
                                     out var lateStart,
                                     out var lateFinish,
                                     work);

                        project = project.SetRaw(TaskFields.LateStart, taskId, lateStart)
                                         .SetRaw(TaskFields.LateFinish, taskId, lateFinish);
                    }
                    else
                    {
                        var lateStart = DateTimeOffset.MaxValue;
                        var lateFinish = DateTimeOffset.MinValue;

                        foreach (var assignmentId in assignmentIds)
                        {
                            var assignmentWork = project.Get(AssignmentFields.Work, assignmentId);
                            var assignmentUnits = project.Get(AssignmentFields.Units, assignmentId);

                            var duration = TimeSpan.FromHours(assignmentWork.TotalHours / assignmentUnits);
                            ComputeStart(project.Information.Calendar,
                                         project.Information.Finish,
                                         successorStart,
                                         constraintType,
                                         constraintDate,
                                         out var assignmentStart,
                                         out var assignmentFinish,
                                         duration);

                            if (assignmentStart < lateStart)
                                lateStart = assignmentStart;

                            if (assignmentFinish > lateFinish)
                                lateFinish = assignmentFinish;
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

            foreach (var taskId in project.Tasks)
            {
                var earlyStart = project.Get(TaskFields.EarlyStart, taskId);
                var earlyFinish = project.Get(TaskFields.EarlyFinish, taskId);
                var lateStart = project.Get(TaskFields.LateStart, taskId);
                var lateFinish = project.Get(TaskFields.LateFinish, taskId);
                var constraintType = project.Get(TaskFields.ConstraintType, taskId);

                // Set start, finish, and duration

                var isAsap = constraintType != ConstraintType.AsLateAsPossible;
                var start = isAsap ? earlyStart : lateStart;
                var finish = isAsap ? earlyFinish : lateFinish;
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

                var minumumEarlyStartOfSuccessors = project.GetSuccessors(taskId)
                                                           .Select(t => project.Get(TaskFields.EarlyStart, t))
                                                           .DefaultIfEmpty(project.Information.Finish)
                                                           .Min();
                var freeSlack = calendar.GetWork(earlyStart, minumumEarlyStartOfSuccessors) - duration;
                if (freeSlack < TimeSpan.Zero ||
                    constraintType == ConstraintType.MustFinishOn ||
                    constraintType == ConstraintType.FinishNoLaterThan)
                    freeSlack = TimeSpan.Zero;
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
                var assignmentFinish = calendar.AddWork(assignmentStart, duration);

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

        private static void ComputeFinish(Calendar calendar,
                                          DateTimeOffset projectStart,
                                          DateTimeOffset? predecessorEnd,
                                          ConstraintType constraintType,
                                          DateTimeOffset? constraintDate,
                                          out DateTimeOffset start,
                                          out DateTimeOffset end,
                                          TimeSpan work)
        {
            start = predecessorEnd ?? projectStart;

            if (work != TimeSpan.Zero)
                start = calendar.FindWorkStart(start);

            if (constraintType == ConstraintType.StartNoEarlierThan && (predecessorEnd == null || start < constraintDate) ||
                constraintType == ConstraintType.StartNoLaterThan && start > constraintDate ||
                constraintType == ConstraintType.MustStartOn)
            {
                start = constraintDate.Value;

                if (constraintType == ConstraintType.StartNoEarlierThan)
                   start = calendar.FindWorkStart(start);
            }

            if (work == TimeSpan.Zero)
                end = start;
            else
                end = calendar.AddWork(start, work);

            if (constraintType == ConstraintType.FinishNoEarlierThan && (predecessorEnd == null || end < constraintDate) ||
                constraintType == ConstraintType.FinishNoLaterThan && end > constraintDate ||
                constraintType == ConstraintType.MustFinishOn)
            {
                end = constraintDate.Value;
                start = calendar.AddWork(end, -work);
            }
        }

        private static void ComputeStart(Calendar calendar,
                                         DateTimeOffset projectEnd,
                                         DateTimeOffset? successorStart,
                                         ConstraintType constraintType,
                                         DateTimeOffset? constraintDate,
                                         out DateTimeOffset start,
                                         out DateTimeOffset end,
                                         TimeSpan work)
        {
            end = successorStart ?? projectEnd;

            if (work != TimeSpan.Zero)
                end = calendar.FindWorkEnd(end);

            if (constraintType == ConstraintType.FinishNoEarlierThan && (/*successorStart == null ||*/ end < constraintDate) ||
                constraintType == ConstraintType.FinishNoLaterThan && (/*successorStart == null ||*/ end > constraintDate) ||
                constraintType == ConstraintType.MustFinishOn)
            {
                end = constraintDate.Value;

                if (constraintType == ConstraintType.FinishNoEarlierThan)
                    end = calendar.FindWorkEnd(end);
            }

            if (work == TimeSpan.Zero)
                start = end;
            else
                start = calendar.AddWork(end, -work);

            if (constraintType == ConstraintType.StartNoEarlierThan && (/*successorStart == null ||*/ start < constraintDate) ||
                constraintType == ConstraintType.StartNoLaterThan && (/*successorStart == null ||*/ start > constraintDate) ||
                constraintType == ConstraintType.MustStartOn)
            {
                start = constraintDate.Value;
                end = calendar.AddWork(start, work);
            }
        }
    }
}
