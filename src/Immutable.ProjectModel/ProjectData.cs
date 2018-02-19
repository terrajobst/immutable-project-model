using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        private readonly ProjectInformationData _information;
        private readonly ImmutableDictionary<TaskId, TaskData> _tasks;
        private readonly ImmutableDictionary<ResourceId, ResourceData> _resources;
        private readonly ImmutableDictionary<AssignmentId, AssignmentData> _assignments;

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
            _information = information;
            _tasks = tasks;
            _resources = resources;
            _assignments = assignments;
        }

        public ProjectData With(ProjectInformationData information,
                                ImmutableDictionary<TaskId, TaskData> tasks,
                                ImmutableDictionary<ResourceId, ResourceData> resources,
                                ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            if (information == _information &&
                tasks == _tasks &&
                resources == _resources &&
                assignments == _assignments)
            {
                return this;
            }

            return new ProjectData(information, tasks, resources, assignments);
        }
    }
}
