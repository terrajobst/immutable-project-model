using System;

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

            if (GetTask(taskId) == null)
                throw new ArgumentException($"The project doesn't contain a task with ID {taskId}.", nameof(taskId));

            if (GetResource(resourceId) == null)
                throw new ArgumentException($"The project doesn't contain a resource with ID {resourceId}.", nameof(resourceId));

            var projectData = Scheduler.AddAssignment(Data, assignmentId, taskId, resourceId);
            return UpdateProject(projectData).GetAssignment(assignmentId);
        }

        public Project RemoveAssignment(AssignmentId assignmentId)
        {
            var projectData = Scheduler.RemoveAssignment(Data, assignmentId);
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
            ProjectData project;

            if (field == AssignmentFields.Work)
            {
                project = SetAssignmentWork(Data, assignment.Id, (TimeSpan)value);
            }
            else
            {
                var assignmentData = assignment.Data.SetValue(field, value);
                project = Data.UpdateAssignment(assignmentData);
            }

            return UpdateProject(project).GetAssignment(assignment.Id);
        }

        private ProjectData SetAssignmentWork(ProjectData project, AssignmentId id, TimeSpan value)
        {
            return Scheduler.SetAssignmentWork(project, project.Assignments[id], value);
        }
    }
}
