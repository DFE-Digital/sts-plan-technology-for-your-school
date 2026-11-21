using Dfe.PlanTech.Core.Enums;
using Dfe.PlanTech.Data.Sql.Entities;

namespace Dfe.PlanTech.Tests.Shared.Builders.Sql;

public sealed class SubmissionEntityBuilder
{
    private static readonly DateTime DefaultDateCreated = new(
        2025,
        1,
        15,
        9,
        0,
        0,
        DateTimeKind.Utc
    );

    private readonly List<ResponseEntity> _responses = [];
    private int _id;
    private int _establishmentId = 1;
    private string _sectionId = "section-id";
    private string _sectionName = "Section name";
    private SubmissionStatus _status = SubmissionStatus.InProgress;
    private DateTime _dateCreated = DefaultDateCreated;
    private DateTime? _dateLastUpdated;
    private DateTime? _dateCompleted;
    private bool _deleted;

    public SubmissionEntityBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public SubmissionEntityBuilder WithEstablishmentId(int establishmentId)
    {
        _establishmentId = establishmentId;
        return this;
    }

    public SubmissionEntityBuilder WithSectionId(string sectionId)
    {
        _sectionId = sectionId;
        return this;
    }

    public SubmissionEntityBuilder WithSectionName(string sectionName)
    {
        _sectionName = sectionName;
        return this;
    }

    public SubmissionEntityBuilder WithStatus(SubmissionStatus status)
    {
        _status = status;
        return this;
    }

    public SubmissionEntityBuilder WithDateCreated(DateTime dateCreated)
    {
        _dateCreated = dateCreated;
        return this;
    }

    public SubmissionEntityBuilder WithDateLastUpdated(DateTime? dateLastUpdated)
    {
        _dateLastUpdated = dateLastUpdated;
        return this;
    }

    public SubmissionEntityBuilder WithDateCompleted(DateTime? dateCompleted)
    {
        _dateCompleted = dateCompleted;
        return this;
    }

    public SubmissionEntityBuilder WithDeleted(bool deleted)
    {
        _deleted = deleted;
        return this;
    }

    public SubmissionEntityBuilder WithResponses(params ResponseEntity[] responses)
    {
        _responses.AddRange(responses);
        return this;
    }

    public SubmissionEntity Build() =>
        new()
        {
            Id = _id,
            EstablishmentId = _establishmentId,
            Establishment = new EstablishmentEntityBuilder().WithId(_establishmentId).Build(),
            SectionId = _sectionId,
            SectionName = _sectionName,
            Status = _status,
            DateCreated = _dateCreated,
            DateLastUpdated = _dateLastUpdated,
            DateCompleted = _dateCompleted,
            Deleted = _deleted,
            Responses = [.. _responses],
        };
}
