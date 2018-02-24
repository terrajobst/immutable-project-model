using System;
using System.Linq;

using Xunit;

namespace Immutable.ProjectModel.Tests.Helpers
{
    internal sealed class ProjectAssert
    {
        private readonly Project _project;

        public static ProjectAssert For(Project project)
        {
            return new ProjectAssert(project);
        }

        public ProjectAssert(Project project)
        {
            _project = project;
        }

        public ProjectAssert HasTask(TaskId id)
        {
            return ForTask(id).Project;
        }

        public ProjectAssert HasNoTask(TaskId id)
        {
            var task = _project.GetTask(id);
            Assert.True(task == null, $"The project {_project.Name} contains a task {id} but shouldn't.");
            return this;
        }

        public TaskAssert ForTask(TaskId id)
        {
            var task = _project.GetTask(id);
            Assert.True(task != null, $"The project {_project.Name} doesn't contain a task with ID {id}");
            return new TaskAssert(this, task);
        }

        public TaskAssert ForTask(int ordinal)
        {
            var task = _project.Tasks.SingleOrDefault(t => t.Ordinal == ordinal);
            Assert.True(task != null, $"The project {_project.Name} doesn't contain a task with ordinal {ordinal}");
            return new TaskAssert(this, task);
        }

        public ProjectAssert HasResource(ResourceId id)
        {
            return ForResource(id).Project;
        }

        public ProjectAssert HasNoResource(ResourceId id)
        {
            var resource = _project.GetResource(id);
            Assert.True(resource == null, $"The project {_project.Name} contains a resource {id} but shouldn't.");
            return this;
        }

        public ProjectAssert HasResource(Predicate<Resource> matcher)
        {
            Assert.Single(_project.Resources, matcher);
            return this;
        }

        public ResourceAssert ForResource(ResourceId id)
        {
            var resource = _project.GetResource(id);
            Assert.True(resource != null, $"The project {_project.Name} doesn't contain a resource with ID {id}");
            return new ResourceAssert(this, resource);
        }

        public ProjectAssert HasAssignment(TaskId taskId, ResourceId resourceId)
        {
            return ForAssignment(taskId, resourceId).Project;
        }

        public ProjectAssert HasNoAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignment = _project.GetAssignment(taskId, resourceId);
            Assert.True(assignment == null, $"The project {_project.Name} contains an assignment for task {taskId} and resource {resourceId} but shouldn't.");
            return this;
        }

        public AssignmentAssert ForAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignment = _project.GetAssignment(taskId, resourceId);
            Assert.True(assignment != null, $"The project {_project.Name} doesn't contain an assignment for task {taskId} and resource {resourceId}");
            return new AssignmentAssert(this, assignment);
        }

        public ProjectAssert AssertFinish(DateTimeOffset value)
        {
            Assert.Equal(value, _project.Finish);
            return this;
        }
    }
}
