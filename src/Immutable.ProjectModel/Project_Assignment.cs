using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public Assignment AddAssignment(TaskId taskId, ResourceId resourceId, AssignmentId assignmentId = default)
        {
            assignmentId = assignmentId.CreateIfDefault();

            if (GetAssignment(assignmentId) != null)
                throw new ArgumentException($"Project already contains an assignment with ID {assignmentId}.");

            if (GetAssignment(taskId, resourceId) != null)
                throw new ArgumentException($"An assignment for task ID {taskId} and resource ID {resourceId} already exists.");

            var task = GetTask(taskId);
            if (task == null)
                throw new ArgumentException($"The project doesn't contain a task with ID {taskId}.", nameof(taskId));

            var resource = GetResource(resourceId);
            if (resource == null)
                throw new ArgumentException($"The project doesn't contain a resource with ID {resourceId}.", nameof(resourceId));

            var projectData = Data.AddAssignment(assignmentId, taskId, resourceId);
            return UpdateProject(projectData).GetAssignment(assignmentId);
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

        internal Assignment SetAssignmentField(Assignment assignment, AssignmentField field, object value)
        {
            Debug.Assert(assignment != null);
            Debug.Assert(field != null);

            var project = Data.Set(field, assignment.Id, value);
            return UpdateProject(project).GetAssignment(assignment.Id);
        }
    }
}
