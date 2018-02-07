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

            var task = GetTask(taskId);
            if (task == null)
                throw new ArgumentException($"The project doesn't contain a task with ID {taskId}.", nameof(taskId));

            var resource = GetResource(resourceId);
            if (resource == null)
                throw new ArgumentException($"The project doesn't contain a resource with ID {resourceId}.", nameof(resourceId));

            var projectData = Scheduler.AddAssignment(Data, assignmentId, taskId, resourceId);

            var assignmentData = projectData.Assignments[assignmentId];

            assignmentData = assignmentData.SetValue(AssignmentFields.TaskName, task.Name)
                                           .SetValue(AssignmentFields.ResourceName, resource.Name);

            projectData = projectData.UpdateAssignment(assignmentData);
            projectData = InitializeTaskResourceNames(projectData, taskId);

            return UpdateProject(projectData).GetAssignment(assignmentId);
        }

        public Project RemoveAssignment(AssignmentId assignmentId)
        {
            var assignment = GetAssignment(assignmentId);
            if (assignment == null)
                return this;

            var taskId = assignment.Task.Id;
            var projectData = Scheduler.RemoveAssignment(Data, assignmentId);
            projectData = InitializeTaskResourceNames(projectData, taskId);

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
            else if (field == AssignmentFields.Units)
            {
                project = SetAssignmentUnits(Data, assignment.Id, (double)value);
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

        private ProjectData SetAssignmentUnits(ProjectData project, AssignmentId id, double value)
        {
            project = project.UpdateAssignment(project.Assignments[id].SetValue(AssignmentFields.Units, value));
            project = InitializeTaskResourceNames(project, project.Assignments[id].TaskId);
            return project;
        }
    }
}
