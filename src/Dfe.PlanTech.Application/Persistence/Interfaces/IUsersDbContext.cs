using System;
using Dfe.PlanTech.Domain.Users.Models;

namespace Dfe.PlanTech.Application.Persistence.Interfaces;

	public interface IUsersDbContext
	{
	public Task<bool> AddUserAsync(User user);
}

