using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        private readonly ProjectInformationData _information;
        private readonly ImmutableDictionary<TaskId, TaskData> _taskMap;
        private readonly ImmutableDictionary<ResourceId, ResourceData> _resourceMap;
        private readonly ImmutableDictionary<AssignmentId, AssignmentData> _assignmentMap;

        public static ProjectData Create(ProjectId id)
        {
            return new ProjectData(ProjectInformationData.Create(id),
                                   ImmutableDictionary<TaskId, TaskData>.Empty,
                                   ImmutableDictionary<ResourceId, ResourceData>.Empty,
                                   ImmutableDictionary<AssignmentId, AssignmentData>.Empty);
        }

        private ProjectData(ProjectInformationData information,
                            ImmutableDictionary<TaskId, TaskData> taskMap,
                            ImmutableDictionary<ResourceId, ResourceData> resourceMap,
                            ImmutableDictionary<AssignmentId, AssignmentData> assignmentMap)
        {
            _information = information;
            _taskMap = taskMap;
            _resourceMap = resourceMap;
            _assignmentMap = assignmentMap;
        }

        private ProjectData With(ProjectInformationData information,
                                 ImmutableDictionary<TaskId, TaskData> taskMap,
                                 ImmutableDictionary<ResourceId, ResourceData> resourceMap,
                                 ImmutableDictionary<AssignmentId, AssignmentData> assignmentMap)
        {
            if (information == _information &&
                taskMap == _taskMap &&
                resourceMap == _resourceMap &&
                assignmentMap == _assignmentMap)
            {
                return this;
            }

            return new ProjectData(information, taskMap, resourceMap, assignmentMap);
        }
    }
}
