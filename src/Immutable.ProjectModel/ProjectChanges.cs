using System.Collections.Immutable;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed class ProjectChanges
    {
        internal static ProjectChanges Compute(ProjectData oldProject, ProjectData newProject)
        {
            // Tasks

            var oldTasks = oldProject.TaskMap;
            var newTasks = newProject.TaskMap;

            var addedTasks = newTasks.Keys
                                     .Where(id => !oldTasks.ContainsKey(id))
                                     .ToImmutableArray();

            var removedTasks = oldTasks.Keys
                                       .Where(id => !newTasks.ContainsKey(id))
                                       .ToImmutableArray();

            var changedTasks = oldTasks.Where(kv => newTasks.ContainsKey(kv.Key))
                                       .Select(kv => (oldTask: kv.Value, newTask: newTasks[kv.Key]))
                                       .Where(t => t.oldTask != t.newTask)
                                       .Select(t => TaskChanges.Compute(t.oldTask, t.newTask))
                                       .Where(c => c.FieldChanges.Any())
                                       .ToImmutableArray();

            // Resources

            var oldResources = oldProject.ResourceMap;
            var newResources = newProject.ResourceMap;

            var addedResources = newResources.Keys
                                             .Where(id => !oldResources.ContainsKey(id))
                                             .ToImmutableArray();

            var removedResources = oldResources.Keys
                                           .Where(id => !newResources.ContainsKey(id))
                                           .ToImmutableArray();

            var changedResources = oldResources.Where(kv => newResources.ContainsKey(kv.Key))
                                               .Select(kv => (oldResource: kv.Value, newResource: newResources[kv.Key]))
                                               .Where(t => t.oldResource != t.newResource)
                                               .Select(t => ResourceChanges.Compute(t.oldResource, t.newResource))
                                               .Where(c => c.FieldChanges.Any())
                                               .ToImmutableArray();

            // Assignments

            var oldAssignments = oldProject.AssignmentMapping;
            var newAssignments = newProject.AssignmentMapping;

            var addedAssignments = newAssignments.Keys
                                                 .Where(id => !oldAssignments.ContainsKey(id))
                                                 .ToImmutableArray();

            var removedAssignments = oldAssignments.Keys
                                                   .Where(id => !newAssignments.ContainsKey(id))
                                                   .ToImmutableArray();

            var changedAssignments = oldAssignments.Where(kv => newAssignments.ContainsKey(kv.Key))
                                                   .Select(kv => (oldAssignment: kv.Value, newAssignment: newAssignments[kv.Key]))
                                                   .Where(t => t.oldAssignment != t.newAssignment)
                                                   .Select(t => AssignmentChanges.Compute(t.oldAssignment, t.newAssignment))
                                                   .Where(c => c.FieldChanges.Any())
                                                   .ToImmutableArray();

            return new ProjectChanges(addedTasks, removedTasks, changedTasks,
                                      addedResources, removedResources, changedResources,
                                      addedAssignments, removedAssignments, changedAssignments);
        }

        internal ProjectChanges(ImmutableArray<TaskId> addedTasks,
                                ImmutableArray<TaskId> removedTasks,
                                ImmutableArray<TaskChanges> changedTasks,
                                ImmutableArray<ResourceId> addedResources,
                                ImmutableArray<ResourceId> removedResources,
                                ImmutableArray<ResourceChanges> changedResources,
                                ImmutableArray<AssignmentId> addedAssignments,
                                ImmutableArray<AssignmentId> removedAssignments,
                                ImmutableArray<AssignmentChanges> changedAssignments)
        {
            AddedTasks = addedTasks;
            RemovedTasks = removedTasks;
            ChangedTasks = changedTasks;
            AddedResources = addedResources;
            RemovedResources = removedResources;
            ChangedResources = changedResources;
            AddedAssignments = addedAssignments;
            RemovedAssignments = removedAssignments;
            ChangedAssignments = changedAssignments;
        }

        public ImmutableArray<TaskId> AddedTasks { get; }
        public ImmutableArray<TaskId> RemovedTasks { get; }
        public ImmutableArray<TaskChanges> ChangedTasks { get; }

        public ImmutableArray<ResourceId> AddedResources { get; }
        public ImmutableArray<ResourceId> RemovedResources { get; }
        public ImmutableArray<ResourceChanges> ChangedResources { get; }

        public ImmutableArray<AssignmentId> AddedAssignments { get; }
        public ImmutableArray<AssignmentId> RemovedAssignments { get; }
        public ImmutableArray<AssignmentChanges> ChangedAssignments { get; }
    }
}
