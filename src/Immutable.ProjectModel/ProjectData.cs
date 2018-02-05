using System.Collections.Immutable;
using System.Linq;

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

        private ProjectData WithTasks(ImmutableDictionary<TaskId, TaskData> tasks)
        {
            return With(Information, tasks, Resources, Assignments);
        }

        public ProjectData AddTask(TaskData task)
        {
            return WithTasks(Tasks.Add(task.Id, task));
        }

        public ProjectData RemoveTask(TaskId taskId)
        {
            var newTasks = Tasks.Remove(taskId);

            var ordinal = 0;

            foreach (var task in Tasks.Values.OrderBy(t => t.Ordinal))
            {
                if (task.Id == taskId)
                    continue;

                var newTask = task;

                // Update Ordinal
                newTask = newTask.SetValue(TaskFields.Ordinal, ordinal);
                ordinal++;

                // Update PredecessorIds
                var predecessors = newTask.PredecessorIds.Remove(taskId);
                newTask = newTask.SetValue(TaskFields.PredecessorIds, predecessors);

                newTasks = newTasks.SetItem(newTask.Id, newTask);
            }

            var newAssignments = Assignments;

            foreach (var assignment in Assignments.Values.Where(a => a.TaskId == taskId))
                newAssignments = newAssignments.Remove(assignment.Id);

            return With(Information, newTasks, Resources, newAssignments);
        }

        public ProjectData UpdateTask(TaskData task)
        {
            return WithTasks(Tasks.SetItem(task.Id, task));
        }

        private ProjectData WithResources(ImmutableDictionary<ResourceId, ResourceData> resources)
        {
            return With(Information, Tasks, resources, Assignments);
        }

        public ProjectData AddResource(ResourceData resource)
        {
            return WithResources(Resources.Add(resource.Id, resource));
        }

        public ProjectData RemoveResource(ResourceId resourceId)
        {
            var newResources = Resources.Remove(resourceId);
            var result = WithResources(newResources);

            foreach (var assignment in Assignments.Values.Where(a => a.ResourceId == resourceId))
                result = Scheduler.RemoveAssignment(result, assignment.Id);

            return result;
        }

        public ProjectData UpdateResource(ResourceData resource)
        {
            return WithResources(Resources.SetItem(resource.Id, resource));
        }

        private ProjectData WithAssignments(ImmutableDictionary<AssignmentId, AssignmentData> assignments)
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
