﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class Project
    {
        private ImmutableDictionary<TaskData, Task> _tasks = ImmutableDictionary<TaskData, Task>.Empty;
        private ImmutableDictionary<ResourceData, Resource> _resources = ImmutableDictionary<ResourceData, Resource>.Empty;
        private ImmutableDictionary<AssignmentData, Assignment> _assignments = ImmutableDictionary<AssignmentData, Assignment>.Empty;

        public static Project Empty = new Project(ProjectData.Empty);

        private Project(ProjectData data)
        {
            Data = data;
        }

        internal ProjectData Data { get; }

        public IEnumerable<Task> Tasks => Data.Tasks.Keys.Select(GetTask).OrderBy(t => t.Ordinal);

        public IEnumerable<Resource> Resources => Data.Resources.Keys.Select(GetResource);

        public IEnumerable<Assignment> Assignments => Data.Assignments.Keys.Select(GetAssignment);

        public Task GetTask(TaskId id)
        {
            if (!Data.Tasks.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _tasks, data, k => new Task(k, this));
        }

        public Resource GetResource(ResourceId id)
        {
            if (!Data.Resources.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _resources, data, k => new Resource(k, this));
        }

        public Assignment GetAssignment(AssignmentId id)
        {
            if (!Data.Assignments.TryGetValue(id, out var data))
                return null;

            return ImmutableInterlocked.GetOrAdd(ref _assignments, data, k => new Assignment(k, this));
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
            Debug.Assert(Scheduler.Schedule(data) == data);

            return new Project(data);
        }

        public Task AddNewTask(TaskId taskId = default)
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
            var projectData = Data.RemoveTask(taskId);
            return UpdateProject(projectData);
        }

        internal Task UpdateTask(TaskData taskData)
        {
            var data = Data.UpdateTask(taskData);
            return UpdateProject(data).GetTask(taskData.Id);
        }

        public Resource AddNewResource(ResourceId resourceId = default)
        {
            resourceId = resourceId.CreateIfDefault();

            if (GetResource(resourceId) != null)
                throw new ArgumentException($"Project already contains a resource with ID {resourceId}.");

            var resourceData = ResourceData.Create(resourceId);
            var projectData = Data.AddResource(resourceData);
            return UpdateProject(projectData).GetResource(resourceData.Id);
        }

        public Project RemoveResource(ResourceId resourceId)
        {
            var projectData = Data.RemoveResource(resourceId);
            return UpdateProject(projectData);
        }

        internal Resource UpdateResource(ResourceData resourceData)
        {
            var data = Data.UpdateResource(resourceData);
            return UpdateProject(data).GetResource(resourceData.Id);
        }

        public Assignment AddNewAssignment(TaskId taskId, ResourceId resourceId, AssignmentId assignmentId = default)
        {
            assignmentId = assignmentId.CreateIfDefault();

            if (GetAssignment(assignmentId) != null)
                throw new ArgumentException($"Project already contains an assignment with ID {assignmentId}.");

            if (GetAssignment(taskId, resourceId) != null)
                throw new ArgumentException($"An assignment for task ID {taskId} and resource ID {resourceId} already exists.");

            if (GetTask(taskId) == null)
                throw new ArgumentException($"The project doesn't contain a task with ID {taskId}.", nameof(taskId));

            if (GetResource(resourceId) == null)
                throw new ArgumentException($"The project doesn't contain a resource with ID {resourceId}.", nameof(resourceId));

            var assignmentData = AssignmentData.Create(assignmentId, taskId, resourceId);
            var projectData = Scheduler.InitializeAssignment(Data.AddAssignment(assignmentData), assignmentData);
            return UpdateProject(projectData).GetAssignment(assignmentData.Id);
        }

        public Project RemoveAssignment(AssignmentId assignmentId)
        {
            var projectData = Data.RemoveAssignment(assignmentId);
            return UpdateProject(projectData);
        }

        public Project RemoveAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignment = GetAssignment(taskId, resourceId);
            if (assignment == null)
                return this;

            return RemoveAssignment(assignment.Id);
        }

        internal Assignment UpdateAssignment(AssignmentData assignmentData)
        {
            var data = Data.UpdateAssignment(assignmentData);
            return UpdateProject(data).GetAssignment(assignmentData.Id);
        }
    }
}
