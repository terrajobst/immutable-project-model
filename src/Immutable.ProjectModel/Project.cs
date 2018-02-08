using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        private ImmutableDictionary<TaskData, Task> _tasks = ImmutableDictionary<TaskData, Task>.Empty;
        private ImmutableDictionary<ResourceData, Resource> _resources = ImmutableDictionary<ResourceData, Resource>.Empty;
        private ImmutableDictionary<AssignmentData, Assignment> _assignments = ImmutableDictionary<AssignmentData, Assignment>.Empty;

        public static Project Create(ProjectId id = default)
        {
            id = id.CreateIfDefault();
            var data = ProjectData.Create(id);
            return new Project(data);
        }

        private Project(ProjectData data)
        {
            Data = data;
        }

        internal ProjectData Data { get; }

        public ProjectId Id => Data.Information.Id;

        public string Name => Data.Information.Name;

        public DateTimeOffset StartDate => Data.Information.StartDate;

        public Calendar Calendar => Data.Information.Calendar;

        public TimeConversion TimeConversion => Data.Information.TimeConversion;

        public IEnumerable<Task> Tasks => Data.Tasks.Keys.Select(GetTask).OrderBy(t => t.Ordinal);

        public IEnumerable<Resource> Resources => Data.Resources.Keys.Select(GetResource);

        public IEnumerable<Assignment> Assignments => Data.Assignments.Keys.Select(GetAssignment);

        public Task GetTask(TaskId id)
        {
            if (!Data.Tasks.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _tasks, data, k => new Task(this, k));
        }

        public Resource GetResource(ResourceId id)
        {
            if (!Data.Resources.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _resources, data, k => new Resource(this, k));
        }

        public Assignment GetAssignment(AssignmentId id)
        {
            if (!Data.Assignments.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _assignments, data, k => new Assignment(this, k));
        }

        public Assignment GetAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignmentData = Data.Assignments.Values.SingleOrDefault(a => a.TaskId == taskId &&
                                                                              a.ResourceId == resourceId);
            if (assignmentData == null)
                return null;

            return GetAssignment(assignmentData.Id);
        }

        public ProjectChanges GetChanges(Project baseline)
        {
            if (baseline == null)
                throw new ArgumentNullException(nameof(baseline));

            return ProjectChanges.Compute(baseline.Data, Data);
        }

        internal Project UpdateProject(ProjectData data)
        {
            if (data == Data)
                return this;

            data = Scheduler.Schedule(data);
            Debug.Assert(Scheduler.Schedule(data) == data, "scheduling isn't idempotent");

            return new Project(data);
        }

        public Project WithName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var information = Data.Information.WithName(name);
            var data = Data.WithInformation(information);
            return UpdateProject(data);
        }

        public Project WithStartDate(DateTimeOffset startDate)
        {
            var information = Data.Information.WithStartDate(startDate);
            var data = Data.WithInformation(information);
            return UpdateProject(data);
        }

        public Project WithCalendar(Calendar calendar)
        {
            if (calendar == null)
                throw new ArgumentNullException(nameof(calendar));

            var information = Data.Information.WithCalendar(calendar);
            var data = Data.WithInformation(information);
            return UpdateProject(data);
        }

        public Project WithTimeConversion(TimeConversion timeConversion)
        {
            if (timeConversion == null)
                throw new ArgumentNullException(nameof(timeConversion));

            var information = Data.Information.WithTimeConversion(timeConversion);
            var data = Data.WithInformation(information);
            return UpdateProject(data);
        }
    }
}
