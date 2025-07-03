namespace Dfe.PlanTech.Domain.Groups.Interfaces;

public interface IRecordGroupSelectionCommand
{
    /// <summary>
    /// Record a Group user's selection of an establishment to view
    /// </summary>
    Task<int> RecordGroupSelection(SubmitSelectionDto submitSelectionDto, CancellationToken cancellationToken = default);

}
