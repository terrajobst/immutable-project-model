using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed class ProjectData
    {
        public static ProjectData Create(ProjectId id)
        {
            return new ProjectData(ProjectInformationData.Create(id),
                                   ImmutableDictionary<TaskId, TaskData>.Empty,
                                   ImmutableDictionary<ResourceId, ResourceData>.Empty,
                                   ImmutableDictionary<AssignmentId, AssignmentData>.Empty);
        }

        private ProjectData(ProjectInformationData information,
                            ImmutableDictionary<TaskId, TaskData> tasks,
                            ImmutableDictionary<ResourceId, ResourceData> resources,
                            ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            Information = information;
            Tasks = tasks;
            Resources = resources;
            Assignments = assignments;
        }

        public ProjectInformationData Information { get; }

        public ImmutableDictionary<TaskId, TaskData> Tasks { get; }

        public ImmutableDictionary<ResourceId, ResourceData> Resources { get; }

        public ImmutableDictionary<AssignmentId, AssignmentData> Assignments { get; }

        public ProjectData With(ProjectInformationData information,
                                ImmutableDictionary<TaskId, TaskData> tasks,
                                ImmutableDictionary<ResourceId, ResourceData> resources,
                                ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            if (information == Information &&
                tasks == Tasks &&
                resources == Resources &&
                assignments == Assignments)
            {
                return this;
            }

            return new ProjectData(information, tasks, resources, assignments);
        }

        public ProjectData WithInformation(ProjectInformationData information)
        {
            return With(information, Tasks, Resources, Assignments);
        }

        public ProjectData WithTasks(ImmutableDictionary<TaskId, TaskData> tasks)
        {
            return With(Information, tasks, Resources, Assignments);
        }

        public ProjectData AddTask(TaskData task)
        {
            return WithTasks(Tasks.Add(task.Id, task));
        }

        public ProjectData RemoveTask(TaskId taskId)
        {
            return WithTasks(Tasks.Remove(taskId));
        }

        public ProjectData UpdateTask(TaskData task)
        {
            return WithTasks(Tasks.SetItem(task.Id, task));
        }

        public ProjectData WithResources(ImmutableDictionary<ResourceId, ResourceData> resources)
        {
            return With(Information, Tasks, resources, Assignments);
        }

        public ProjectData AddResource(ResourceData resource)
        {
            return WithResources(Resources.Add(resource.Id, resource));
        }

        public ProjectData RemoveResource(ResourceId resourceId)
        {
            return WithResources(Resources.Remove(resourceId));
        }

        public ProjectData UpdateResource(ResourceData resource)
        {
            return WithResources(Resources.SetItem(resource.Id, resource));
        }

        public ProjectData WithAssignments(ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            return With(Information, Tasks, Resources, assignments);
        }

        public ProjectData AddAssignment(AssignmentData assignment)
        {
            return WithAssignments(Assignments.Add(assignment.Id, assignment));
        }

        public ProjectData RemoveAssignment(AssignmentId assignmentId)
        {
            return WithAssignments(Assignments.Remove(assignmentId));
        }

        public ProjectData UpdateAssignment(AssignmentData assignment)
        {
            return WithAssignments(Assignments.SetItem(assignment.Id, assignment));
        }
    }
}
