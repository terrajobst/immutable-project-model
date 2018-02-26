using Immutable.ProjectModel.Tests.Helpers;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class ResourceTests
    {
        [Fact]
        public void Resource_Name_Set()
        {
            var resourceId = ResourceId.Create();
            var project = Project.Create()
                                 .AddResource(resourceId)
                                    .WithName("Immo")
                                    .Project;

            ProjectAssert.For(project)
                         .ForResource(resourceId)
                             .AssertName("Immo");
        }
    }
}
