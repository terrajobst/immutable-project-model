using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Immutable.ProjectModel
{
    public static class ResourceFields
    {
        public static readonly ResourceField<ResourceId> Id = new ResourceField<ResourceId>("Resource.Id", FieldKinds.ResourceId, FieldFlags.ReadOnly | FieldFlags.Virtual);
        public static readonly ResourceField<string> Name = new ResourceField<string>("Resource.Name", FieldKinds.Text);

        public static readonly ImmutableArray<ResourceField> All = typeof(ResourceFields).GetFields(BindingFlags.Static | BindingFlags.Public)
                                                                                         .Select(f => f.GetValue(null))
                                                                                         .OfType<ResourceField>()
                                                                                         .ToImmutableArray();
    }
}
