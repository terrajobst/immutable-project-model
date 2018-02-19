using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<ResourceId, ResourceData> Resources => _resources;

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
    }
}
