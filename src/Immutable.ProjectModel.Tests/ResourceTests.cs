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

        [Fact]
        public void Resource_Initials_Set()
        {
            var resourceId = ResourceId.Create();
            var project = Project.Create()
                                 .AddResource(resourceId)
                                    .WithName("Immo")
                                    .WithInitials("IL")
                                    .Project;

            ProjectAssert.For(project)
                         .ForResource(resourceId)
                             .AssertInitials("IL");
        }

        [Fact]
        public void Resource_Initials_InitializedFromName()
        {
            var resourceId = ResourceId.Create();
            var project = Project.Create()
                                 .AddResource(resourceId)
                                    .WithName("Immo")
                                    .Project;

            ProjectAssert.For(project)
                         .ForResource(resourceId)
                             .AssertInitials("I");
        }

        [Fact]
        public void Resource_Initials_NotModified()
        {
            var resourceId = ResourceId.Create();
            var project = Project.Create()
                                 .AddResource(resourceId)
                                    .WithName("Immo")
                                    .WithName("Thomas")
                                    .Project;

            ProjectAssert.For(project)
                         .ForResource(resourceId)
                             .AssertInitials("I");
        }

        [Fact]
        public void Resource_Initials_NotModified_EvenIfEmpty()
        {
            var resourceId = ResourceId.Create();
            var project = Project.Create()
                                 .AddResource(resourceId)
                                    .WithName("Immo")
                                    .WithInitials(string.Empty)
                                    .WithName("Thomas")
                                    .Project;

            ProjectAssert.For(project)
                         .ForResource(resourceId)
                             .AssertInitials(string.Empty);
        }
    }
}
