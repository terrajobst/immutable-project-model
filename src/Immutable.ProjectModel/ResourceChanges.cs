using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Immutable.ProjectModel
{
    public sealed class ResourceChanges
    {
        internal static ResourceChanges Compute(ResourceData oldResource, ResourceData newResource)
        {
            Debug.Assert(oldResource != newResource);
            Debug.Assert(oldResource.Id == newResource.Id);

            var result = new List<FieldChange<ResourceField>>();

            foreach (var field in oldResource.SetFields)
            {
                var oldValue = oldResource.GetValue(field);

                if (!newResource.HasValue(field))
                {
                    result.Add(new FieldChange<ResourceField>(field, oldValue, field.DefaultValue));
                }
                else
                {
                    var newValue = newResource.GetValue(field);
                    if (!Equals(oldValue, newValue))
                        result.Add(new FieldChange<ResourceField>(field, oldValue, newValue));
                }
            }

            foreach (var field in newResource.SetFields)
            {
                var newValue = newResource.GetValue(field);

                if (!oldResource.HasValue(field))
                {
                    result.Add(new FieldChange<ResourceField>(field, field.DefaultValue, newValue));
                }
            }

            return new ResourceChanges(oldResource.Id, result.ToImmutableArray());
        }

        private ResourceChanges(ResourceId id, ImmutableArray<FieldChange<ResourceField>> fieldChanges)
        {
            Id = id;
            FieldChanges = fieldChanges;
        }

        public ResourceId Id { get; }

        public ImmutableArray<FieldChange<ResourceField>> FieldChanges { get; }
    }
}
