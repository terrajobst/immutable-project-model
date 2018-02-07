using System;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public Resource AddNewResource(ResourceId resourceId = default)
        {
            resourceId = resourceId.CreateIfDefault();

            if (GetResource(resourceId) != null)
                throw new ArgumentException($"Project already contains a resource with ID {resourceId}.");

            var resourceData = ResourceData.Create(resourceId);
            var projectData = Data.AddResource(resourceData);
            return UpdateProject(projectData).GetResource(resourceData.Id);
        }

        public Project RemoveResource(ResourceId resourceId)
        {
            var projectData = Data.RemoveResource(resourceId);
            return UpdateProject(projectData);
        }

        internal Resource SetResourceField(Resource resource, ResourceField field, object value)
        {
            ProjectData project;

            var resourceData = resource.Data.SetValue(field, value);
            project = Data.UpdateResource(resourceData);

            return UpdateProject(project).GetResource(resource.Id);
        }
    }
}
