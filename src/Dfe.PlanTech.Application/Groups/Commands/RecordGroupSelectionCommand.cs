using System.Data;
using System.Security.Authentication;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Domain.Groups.Interfaces;
using Dfe.PlanTech.Domain.Groups.Models;
using Dfe.PlanTech.Domain.Users.Interfaces;
using Microsoft.Data.SqlClient;

namespace Dfe.PlanTech.Application.Groups.Commands;

public class RecordGroupSelectionCommand : IRecordGroupSelectionCommand
{
    private readonly IPlanTechDbContext _db;
    private readonly IUser _user;
    private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;
    private readonly ICreateEstablishmentCommand _createEstablishmentCommand;

    public RecordGroupSelectionCommand(IPlanTechDbContext db, IUser user, IGetEstablishmentIdQuery getEstablishmentIdQuery, ICreateEstablishmentCommand createEstablishmentCommand)
    {
        _db = db;
        _user = user;
        _getEstablishmentIdQuery = getEstablishmentIdQuery;
        _createEstablishmentCommand = createEstablishmentCommand;
    }

    public async Task<int> RecordGroupSelection(SubmitSelectionDto submitSelectionDto, CancellationToken cancellationToken = default)
    {
        if (submitSelectionDto?.SelectedEstablishmentUrn == null)
            throw new InvalidDataException($"{nameof(submitSelectionDto.SelectedEstablishmentUrn)} is null");

        int userId = await _user.GetCurrentUserId() ?? throw new AuthenticationException("User is not authenticated");
        int establishmentId = await _user.GetEstablishmentId();
        var selectedEstablishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(submitSelectionDto.SelectedEstablishmentUrn);

        if (selectedEstablishmentId is null)
        {
            var establishment = new EstablishmentDto()
            {
                OrgName = submitSelectionDto.SelectedEstablishmentName,
                Urn = submitSelectionDto.SelectedEstablishmentUrn
            };

            selectedEstablishmentId = await _createEstablishmentCommand.CreateEstablishment(establishment);
        }

        var dto = new GroupSelectionDto()
        {
            UserId = userId,
            UserEstablishment = establishmentId,
            SelectedEstablishmentId = (int)selectedEstablishmentId,
            SelectedEstablishmentName = submitSelectionDto.SelectedEstablishmentName
        };

        var selectionId = await RecordSelection(dto, cancellationToken);

        return selectionId;
    }

    private async Task<int> RecordSelection(GroupSelectionDto groupSelectionDto, CancellationToken cancellationToken)
    {
        var establishmentId = new SqlParameter("@establishmentId", groupSelectionDto.UserEstablishment);
        var userId = new SqlParameter("@userId", groupSelectionDto.UserId);
        var selectedEstablishmentId = new SqlParameter("@selectedEstablishmentId", groupSelectionDto.SelectedEstablishmentId);
        var selectedEstablishmentName = new SqlParameter("@selectedEstablishmentName", groupSelectionDto.SelectedEstablishmentName);

        var selectionId = new SqlParameter("@selectionId", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        await _db.ExecuteSqlAsync($@"EXEC SubmitGroupSelection
                                            @establishmentId={establishmentId},
                                            @userId={userId},
                                            @selectedEstablishmentId={selectedEstablishmentId},
                                            @selectedEstablishmentName={selectedEstablishmentName},
                                           
                                            @selectionId={selectionId} OUTPUT",
                                            cancellationToken);

        if (selectionId.Value is int id)
        {
            return id;
        }

        throw new InvalidCastException($"responseId is not int - is {selectionId.Value}");

    }
}
