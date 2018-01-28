using System.Collections.Immutable;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed class ProjectData
    {
        public static ProjectData Empty = new ProjectData(ImmutableDictionary<TaskId, TaskData>.Empty,
                                                          ImmutableDictionary<ResourceId, ResourceData>.Empty,
                                                          ImmutableDictionary<AssignmentId, AssignmentData>.Empty);

        private ProjectData(ImmutableDictionary<TaskId, TaskData> tasks,
                            ImmutableDictionary<ResourceId, ResourceData> resources,
                            ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            Tasks = tasks;
            Resources = resources;
            Assignments = assignments;
        }

        public ImmutableDictionary<TaskId, TaskData> Tasks { get; }

        public ImmutableDictionary<ResourceId, ResourceData> Resources { get; }

        public ImmutableDictionary<AssignmentId, AssignmentData> Assignments { get; }

        public ProjectData With(ImmutableDictionary<TaskId, TaskData> tasks,
                                ImmutableDictionary<ResourceId, ResourceData> resources,
                                ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            if (tasks == Tasks &&
                resources == Resources &&
                assignments == Assignments)
            {
                return this;
            }

            return new ProjectData(tasks, resources, assignments);
        }

        private ProjectData WithTasks(ImmutableDictionary<TaskId, TaskData> tasks)
        {
            return With(tasks, Resources, Assignments);
        }

        public ProjectData AddTask(TaskData task)
        {
            return WithTasks(Tasks.Add(task.Id, task));
        }

        public ProjectData RemoveTask(TaskId taskId)
        {
            var newTasks = Tasks.Remove(taskId);

            foreach (var task in Tasks.Values)
            {
                if (task.Id == taskId)
                    continue;

                var predecessors = task.PredecessorIds.Remove(taskId);
                var newTask = task.SetValue(TaskFields.PredecessorIds, predecessors);
                newTasks = newTasks.SetItem(newTask.Id, newTask);
            }

            var newAssignments = Assignments;

            foreach (var assignment in Assignments.Values.Where(a => a.TaskId == taskId))
                newAssignments = newAssignments.Remove(assignment.Id);

            return With(newTasks, Resources, newAssignments);
        }

        public ProjectData UpdateTask(TaskData task)
        {
            return WithTasks(Tasks.SetItem(task.Id, task));
        }

        private ProjectData WithResources(ImmutableDictionary<ResourceId, ResourceData> resources)
        {
            return With(Tasks, resources, Assignments);
        }

        public ProjectData AddResource(ResourceData resource)
        {
            return WithResources(Resources.Add(resource.Id, resource));
        }

        public ProjectData RemoveResource(ResourceId resourceId)
        {
            var newResources = Resources.Remove(resourceId);

            var newAssignments = Assignments;

            foreach (var assignment in Assignments.Values.Where(a => a.ResourceId == resourceId))
                newAssignments = newAssignments.Remove(assignment.Id);

            return With(Tasks, newResources, newAssignments);
        }

        public ProjectData UpdateResource(ResourceData resource)
        {
            return WithResources(Resources.SetItem(resource.Id, resource));
        }

        private ProjectData WithAssignments(ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            return With(Tasks, Resources, assignments);
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
