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

        public TaskAssert HasTask(TaskId taskId)
        {
            var task = _project.GetTask(taskId);
            Assert.True(task != null, $"The project {_project.Name} doesn't contain a task with ID {taskId}");
            return new TaskAssert(this, task);
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

        public AssignmentAssert ForAssignment(TaskId taskId, ResourceId resourceId)
        {
            var assignment = _project.GetAssignment(taskId, resourceId);
            Assert.True(assignment != null, $"The project {_project.Name} doesn't contain an assignment for task {taskId} and resource {resourceId}");
            return new AssignmentAssert(this, assignment);
        }
    }
}
