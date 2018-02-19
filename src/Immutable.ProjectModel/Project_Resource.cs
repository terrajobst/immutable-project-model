using System;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public Resource AddResource(ResourceId resourceId = default)
        {
            resourceId = resourceId.CreateIfDefault();

            if (GetResource(resourceId) != null)
                throw new ArgumentException($"Project already contains a resource with ID {resourceId}.");

            var projectData = Data.AddResource(resourceId);
            return UpdateProject(projectData).GetResource(resourceId);
        }

        public Project RemoveResource(ResourceId resourceId)
        {
            var project = Data.RemoveResource(resourceId);
            return UpdateProject(project);
        }

        internal Resource SetResourceField(Resource resource, ResourceField field, object value)
        {
            Debug.Assert(resource != null);
            Debug.Assert(field != null);

            var project = Data.Set(field, resource.Id, value);
            return UpdateProject(project).GetResource(resource.Id);
        }
    }
}
