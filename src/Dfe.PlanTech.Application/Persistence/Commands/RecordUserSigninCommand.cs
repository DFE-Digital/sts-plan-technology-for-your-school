using System;
using System.Reflection.Emit;
using Dfe.PlanTech.Application.Persistence.Interfaces;
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

        public async Task RecordSignIn(CreateUserDTO createUserDTO)
        {
            var user = new User() { DfeSigninRef = createUserDTO.DfeSigninRef, UserId = createUserDTO.UserId };
            await _db.AddUserAsync(user);
        }
    }
}

