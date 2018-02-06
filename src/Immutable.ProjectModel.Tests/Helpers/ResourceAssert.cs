using Xunit;

namespace Immutable.ProjectModel.Tests.Helpers
{
    internal sealed class ResourceAssert
    {
        private readonly Resource _resource;

        public ResourceAssert(ProjectAssert project, Resource resource)
        {
            Project = project;
            _resource = resource;
        }

        public ProjectAssert Project { get; }

        public ResourceAssert AssertField(ResourceField field, object value)
        {
            var actualValue = _resource.GetValue(field);
            var equals = Equals(value, actualValue);
            Assert.True(equals, $"Resource {_resource.Id} {field.Name} should have been '{value}' but was '{actualValue}'");
            return this;
        }
    }
}
