using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<AssignmentId, AssignmentData> Assignments => _assignments;

        public ProjectData WithAssignments(ImmutableDictionary<AssignmentId, AssignmentData> assignments)
        {
            return With(Information, Tasks, Resources, assignments);
        }

        public ProjectData AddAssignment(AssignmentData assignment)
        {
            return WithAssignments(Assignments.Add(assignment.Id, assignment));
        }

        public ProjectData RemoveAssignment(AssignmentId assignmentId)
        {
            return WithAssignments(Assignments.Remove(assignmentId));
        }

        public ProjectData UpdateAssignment(AssignmentData assignment)
        {
            return WithAssignments(Assignments.SetItem(assignment.Id, assignment));
        }
    }
}
