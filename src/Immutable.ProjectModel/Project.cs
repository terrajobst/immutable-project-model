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

        public DateTimeOffset Start => Data.Information.Start;

        public DateTimeOffset Finish => Data.Information.Finish;

        public Calendar Calendar => Data.Information.Calendar;

        public TimeConversion TimeConversion => Data.Information.TimeConversion;

        public IEnumerable<Task> Tasks => Data.Tasks.Select(GetTask).OrderBy(t => t.Ordinal);

        public IEnumerable<TaskLink> TaskLinks => Data.TaskLinks;

        public IEnumerable<Resource> Resources => Data.Resources.Select(GetResource);

        public IEnumerable<Assignment> Assignments => Data.Assignments.Select(GetAssignment);

        public Task GetTask(TaskId id)
        {
            if (!Data.TaskMap.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _tasks, data, k => new Task(this, k));
        }

        public Resource GetResource(ResourceId id)
        {
            if (!Data.ResourceMap.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _resources, data, k => new Resource(this, k));
        }

        public Assignment GetAssignment(AssignmentId id)
        {
            if (!Data.AssignmentMapping.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _assignments, data, k => new Assignment(this, k));
        }

        public Assignment GetAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignmentId = Data.GetAssignment(taskId, resourceId);
            if (assignmentId.IsDefault)
                return null;

            return GetAssignment(assignmentId);
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

        public Project WithStart(DateTimeOffset start)
        {
            var information = Data.Information.WithStart(start);
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
