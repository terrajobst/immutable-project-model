namespace Immutable.ProjectModel
{
    public enum ConstraintType
    {
        AsSoonAsPossible,
        AsLateAsPossible,
        StartNoEarlierThan,
        StartNoLaterThan,
        FinishNoEarlierThan,
        FinishNoLaterThan,
        MustStartOn,
        MustFinishOn
    }
}
