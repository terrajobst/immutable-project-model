﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal static class Scheduler
    {
        public static ProjectData Schedule(ProjectData project)
        {
            return project.ComputeEarlyStartAndFinish()
                          .ComputeLateStartAndFinish()
                          .ComputeDuration();
        }

        private static ProjectData ComputeEarlyStartAndFinish(this ProjectData project)
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

                    var work = GetWork(project, task);
                    ComputeFinish(project.Information.Calendar, ref earlyStart, out var earlyFinish, work);

                    task = task.SetValue(TaskFields.EarlyStart, earlyStart)
                               .SetValue(TaskFields.EarlyFinish, earlyFinish);

                    project = project.UpdateTask(task);

                    computedTasks.Add(task.Id);
                }
            }

            return project;
        }

        private static ProjectData ComputeLateStartAndFinish(this ProjectData project)
        {
            var computedTasks = new HashSet<TaskId>();
            var successorsById = project.Tasks
                                            .Values
                                            .SelectMany(s => s.PredecessorIds, (s, pId) => (predecessor: pId, successor: s.Id))
                                            .ToLookup(t => t.predecessor, t => t.successor);

            var toBeScheduled = new Queue<TaskData>(project.Tasks.Values);

            while (toBeScheduled.Count > 0)
            {
                var task = toBeScheduled.Dequeue();
                var successors = successorsById[task.Id];
                var allSuccessorsComputed = successors.All(id => computedTasks.Contains(id));
                if (!allSuccessorsComputed)
                {
                    toBeScheduled.Enqueue(task);
                }
                else
                {
                    var lateFinish = successors.Select(id => project.Tasks[id].LateStart)
                                               .DefaultIfEmpty(task.EarlyFinish)
                                               .Min();

                    var work = GetWork(project, task);
                    ComputeStart(project.Information.Calendar, out var lateStart, ref lateFinish, work);

                    task = task.SetValue(TaskFields.LateStart, lateStart)
                               .SetValue(TaskFields.LateFinish, lateFinish);

                    project = project.UpdateTask(task);

                    computedTasks.Add(task.Id);
                }
            }

            return project;
        }

        private static ProjectData ComputeDuration(this ProjectData project)
        {
            var calendar = project.Information.Calendar;
            var tasks = project.Tasks.Values;

            foreach (var task in tasks)
            {
                var work = GetWork(calendar, task.EarlyStart, task.EarlyFinish);
                var duration = TimeSpan.FromDays(work.TotalHours / 8.0);
                var newTask = task.SetValue(TaskFields.Duration, duration);
                project = project.UpdateTask(newTask);
            }

            return project;
        }

        private static TimeSpan GetWork(ProjectData project, TaskData task)
        {
            var hasAssignments = project.Assignments.Values.Any(a => a.TaskId == task.Id);
            if (!hasAssignments || task.Work == TimeSpan.Zero)
                return TimeSpan.FromHours(task.Duration.TotalDays * 8);

            return task.Work;
        }

        private static void ComputeFinish(Calendar calendar, ref DateTimeOffset start, out DateTimeOffset end, TimeSpan work)
        {
            if (work == TimeSpan.Zero)
            {
                end = start;
                return;
            }

            start = AddWork(calendar, start, TimeSpan.Zero);
            end = AddWork(calendar, start, work);
        }

        private static void ComputeStart(Calendar calendar, out DateTimeOffset start, ref DateTimeOffset end, TimeSpan work)
        {
            if (work == TimeSpan.Zero)
            {
                start = end;
                return;
            }

            end = SubtractWork(calendar, end, TimeSpan.Zero);
            start = SubtractWork(calendar, end, work);
        }

        private static DateTimeOffset AddWork(Calendar calendar, DateTimeOffset date, TimeSpan work)
        {
            var result = date;

            do
            {
                var workingTime = GetNextWorkingTime(calendar, ref result);
                if (workingTime > work)
                {
                    workingTime = work;
                }
                else
                {
                    result += workingTime;
                }

                work -= workingTime;
            }
            while (work > TimeSpan.Zero);

            return result;
        }

        private static DateTimeOffset SubtractWork(Calendar calendar, DateTimeOffset date, TimeSpan work)
        {
            var result = date;

            do
            {
                var workingTime = GetPreviousWorkingTime(calendar, ref result);
                if (workingTime > work)
                {
                    workingTime = work;
                }
                else
                {
                    result -= workingTime;
                }

                work -= workingTime;
            }
            while (work > TimeSpan.Zero);

            return result;
        }

        private static TimeSpan GetNextWorkingTime(Calendar calendar, ref DateTimeOffset date)
        {
            while (true)
            {
                var day = calendar.WorkingWeek[date.DayOfWeek];

                foreach (var time in day.WorkingTimes)
                {
                    var from = date.Date.Add(time.From);
                    var to = date.Date.Add(time.To);
                    if (date < to)
                    {
                        date = date < from ? from : date;
                        return to - date;
                    }
                }

                date = date.Date.AddDays(1);
            }
        }

        private static TimeSpan GetPreviousWorkingTime(Calendar calendar, ref DateTimeOffset date)
        {
            var input = date;
            var atEndOfDay = false;

            while (true)
            {
                var day = calendar.WorkingWeek[date.DayOfWeek];

                foreach (var time in day.WorkingTimes.Reverse())
                {
                    var from = date.Date.Add(time.From);
                    var to = date.Date.Add(time.To);

                    if (atEndOfDay)
                        date = to;

                    if (date > from)
                    {
                        date = date > to ? to : date;
                        return date - from;
                    }
                }

                date = date.Date.AddDays(-1);
                atEndOfDay = true;
            }
        }

        private static TimeSpan GetWork(Calendar calendar, DateTimeOffset start, DateTimeOffset end)
        {
            Debug.Assert(start <= end);

            var result = TimeSpan.Zero;
            var date = start;

            while (date < end)
            {
                var workingTime = GetNextWorkingTime(calendar, ref date);
                date += workingTime;

                if (date > end)
                    workingTime -= date - end;

                result += workingTime;
            }

            return result;
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

        public static ProjectData SetAssignmentWork(ProjectData project, AssignmentData assignment, TimeSpan work)
        {
            var task = project.Tasks[assignment.TaskId];
            var assignmentWorkDelata = work - assignment.Work;

            var taskWork = task.Work + assignmentWorkDelata;
            var newTask = task.SetValue(TaskFields.Work, taskWork);
            var newAssignment = assignment.SetValue(AssignmentFields.Work, work);

            return project.UpdateTask(newTask)
                          .UpdateAssignment(newAssignment);
        }

        public static ProjectData InitializeAssignment(ProjectData project, AssignmentData assignment)
        {
            var task = project.Tasks[assignment.TaskId];
            var allAssignments = project.Assignments.Values.Where(a => a.TaskId == assignment.TaskId).ToArray();

            if (allAssignments.Length == 1)
            {
                var work = task.Work != TimeSpan.Zero ? task.Work : task.Duration;
                assignment = assignment.SetValue(AssignmentFields.Work, work);
                project = project.UpdateAssignment(assignment);
            }
            else
            {
                var newTaskWorkHours = task.Work.TotalHours / (allAssignments.Length - 1) * allAssignments.Length;
                var newAssignmentWorkHours = newTaskWorkHours - task.Work.TotalHours;

                var newTaskWork = TimeSpan.FromHours(newTaskWorkHours);
                var newAssignmentWork = TimeSpan.FromHours(newAssignmentWorkHours);

                task = task.SetValue(TaskFields.Work, newTaskWork);
                project = project.UpdateTask(task);

                assignment = assignment.SetValue(AssignmentFields.Work, newAssignmentWork);
                project = project.UpdateAssignment(assignment);
            }

            return project;
        }
    }
}
