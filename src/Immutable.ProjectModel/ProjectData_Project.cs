namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ProjectInformationData Information => _information;

        public ProjectData WithInformation(ProjectInformationData information)
        {
            return With(information, _taskMap, _resourceMap, _assignmentMap);
        }
    }
}
