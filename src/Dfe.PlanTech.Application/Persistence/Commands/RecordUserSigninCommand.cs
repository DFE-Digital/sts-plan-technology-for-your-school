using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Application.Persistence.Queries;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Persistence.Commands
{
    public class RecordUserSigninCommand
    {
        private readonly IUsersDbContext _db;

        public RecordUserSigninCommand(IUsersDbContext db)
        {
            _db = db;
        }

        public async Task RecordSignIn(RecordUserSignInDto recordUserSignInDto)
        {
            //Check user exists already
            var getUserIdQuery = new GetUserIdQuery(_db);

            var existingUserId = await getUserIdQuery.GetUserId(recordUserSignInDto.DfeSignInRef);

            if(existingUserId == null){
                var CreateUserCommand = new CreateUserCommand(_db);
                existingUserId = await CreateUserCommand.CreateUser(recordUserSignInDto);
            }

            //RECORD SIGN IN
        }
    }
}

