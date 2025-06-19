//using Dfe.PlanTech.Application.Persistence.Interfaces;
//using Dfe.PlanTech.Domain.DataTransferObjects;
//using Dfe.PlanTech.Domain.Establishments.Models;
//using Dfe.PlanTech.Domain.SignIns.Models;
//using Dfe.PlanTech.Domain.Users.Interfaces;
//namespace Dfe.PlanTech.Application.Users.Commands;

//public class RecordUserSignInCommand : IRecordUserSignInCommand
//{
//    private readonly IPlanTechDbContext _db;
//    private readonly ICreateEstablishmentCommand _createEstablishmentCommand;
//    private readonly ICreateUserCommand _createUserCommand;
//    private readonly IGetEstablishmentIdQuery _getEstablishmentIdQuery;
//    private readonly IGetUserIdQuery _getUserIdQuery;

//    public RecordUserSignInCommand(IPlanTechDbContext db, ICreateEstablishmentCommand createEstablishmentCommand, ICreateUserCommand createUserCommand, IGetEstablishmentIdQuery getEstablishmentIdQuery, IGetUserIdQuery getUserIdQuery)
//    {
//        _db = db;
//        _createEstablishmentCommand = createEstablishmentCommand;
//        _createUserCommand = createUserCommand;
//        _getEstablishmentIdQuery = getEstablishmentIdQuery;
//        _getUserIdQuery = getUserIdQuery;
//    }

//    public async Task<SignIn> RecordSignIn(RecordUserSignInDto recordUserSignInDto)
//    {
//        var signIn = await CreateSignIn(recordUserSignInDto);

//        await AddSignInDetails(signIn);

//        return signIn;
//    }

//    public async Task<SignIn> RecordSignInUserOnly(string dfeSignInRef)
//    {
//        var userId = await GetUserId(dfeSignInRef);
//        var signIn = MapToSignIn(userId);

//        await AddSignInDetails(signIn);

//        return signIn;
//    }

//    /// <summary>
//    /// Creates SignIn row
//    /// </summary>
//    /// <param name="recordUserSignInDto"></param>
//    /// <returns></returns>
//    private async Task<SignIn> CreateSignIn(RecordUserSignInDto recordUserSignInDto)
//    {
//        int userId = await GetUserId(recordUserSignInDto.DfeSignInRef);
//        var establishmentId = await GetEstablishmentId(recordUserSignInDto);
//        var signIn = MapToSignIn(userId, establishmentId);

//        return signIn;
//    }

//    /// <summary>
//    /// Gets existing ID for user, or creates a new user in database if non existing
//    /// </summary>
//    /// <param name="dfeSignInRef"></param>
//    /// <returns></returns>
//    private async Task<int> GetUserId(string dfeSignInRef)
//    {
//        var existingUserId = await _getUserIdQuery.GetUserId(dfeSignInRef);

//        if (existingUserId != null)
//        {
//            return existingUserId.Value;
//        }

//        return await _createUserCommand.CreateUser(dfeSignInRef);
//    }

//    /// <summary>
//    /// Gets existing ID for user, or creates a new establishment in database if non existing
//    /// </summary>
//    /// <param name="recordUserSignInDto"></param>
//    /// <returns></returns>
//    private async Task<int> GetEstablishmentId(RecordUserSignInDto recordUserSignInDto)
//    {
//        var existingEstablishmentId = await _getEstablishmentIdQuery.GetEstablishmentId(recordUserSignInDto.Organisation.Reference);

//        if (existingEstablishmentId != null)
//        {
//            return existingEstablishmentId.Value;
//        }

//        return await _createEstablishmentCommand.CreateEstablishment(new EstablishmentDto()
//        {
//            Ukprn = recordUserSignInDto.Organisation.Ukprn,
//            Urn = recordUserSignInDto.Organisation.Urn,
//            Type = recordUserSignInDto.Organisation.Type?.Name == null ? null : new EstablishmentTypeDto()
//            {
//                Name = recordUserSignInDto.Organisation.Type.Name
//            },
//            OrgName = recordUserSignInDto.Organisation.Name,
//            GroupUid = recordUserSignInDto.Organisation.Uid
//        });
//    }

//    private static SignIn MapToSignIn(int userId, int? establishmentId = null)
//    {
//        if (userId == 0)
//            throw new ArgumentNullException(nameof(userId), "User id cannot be null");

//        return new SignIn
//        {
//            UserId = userId,
//            EstablishmentId = establishmentId,
//            SignInDateTime = DateTime.UtcNow
//        };
//    }

//    /// <summary>
//    /// Record SignIn in database
//    /// </summary>
//    /// <param name="signIn"></param>
//    /// <returns>Id of created SignIn</returns>
//    private async Task<int> AddSignInDetails(SignIn signIn)
//    {
//        _db.AddSignIn(signIn);
//        await _db.SaveChangesAsync();

//        return signIn.Id;
//    }
//}
