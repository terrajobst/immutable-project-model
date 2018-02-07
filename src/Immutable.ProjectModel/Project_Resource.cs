namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        internal Resource SetResourceField(Resource resource, ResourceField field, object value)
        {
            ProjectData project;

            var resourceData = resource.Data.SetValue(field, value);
            project = Data.UpdateResource(resourceData);

            return UpdateProject(project).GetResource(resource.Id);
        }
    }
}
