using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Xunit;

namespace Immutable.ProjectModel.Tests
{
    public class FieldTests
    {
        [Fact]
        public void Task_AllFields_HaveProperties()
        {
            AssertFieldsHaveProperties(typeof(Task), TaskFields.All);
        }

        [Fact]
        public void Task_AllFields_HaveWithers()
        {
            AssertFieldsHaveWithers(typeof(Task), TaskFields.All);
        }

        [Fact]
        public void Resource_AllFields_HaveProperties()
        {
            AssertFieldsHaveProperties(typeof(Resource), ResourceFields.All);
        }

        [Fact]
        public void Resource_AllFields_HaveWithers()
        {
            AssertFieldsHaveWithers(typeof(Resource), ResourceFields.All);
        }

        [Fact]
        public void Assignment_AllFields_HaveProperties()
        {
            AssertFieldsHaveProperties(typeof(Assignment), AssignmentFields.All);
        }

        [Fact]
        public void Assignment_AllFields_HaveWithers()
        {
            AssertFieldsHaveWithers(typeof(Assignment), AssignmentFields.All);
        }

        private static void AssertFieldsHaveProperties(Type type, IEnumerable<FieldDefinition> fields)
        {
            var properties = new HashSet<string>(type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                     .Where(p => p.GetIndexParameters().Length == 0)
                                                     .Select(p => p.Name));

            foreach (var f in fields)
            {
                Assert.Contains(f.Name, properties);
            }
        }

        private static void AssertFieldsHaveWithers(Type type, IEnumerable<FieldDefinition> fields)
        {
            var withers = new HashSet<string>(type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                  .Where(m => m.Name.StartsWith("With"))
                                                  .Select(m => m.Name.Substring("With".Length)));

            foreach (var f in fields)
            {
                if (!f.IsReadOnly)
                    Assert.Contains(f.Name, withers);
            }
        }
    }
}
