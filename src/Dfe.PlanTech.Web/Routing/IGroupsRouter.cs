using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Establishments.Models;
using Dfe.PlanTech.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.PlanTech.Web.Routing;

/// <summary>
/// Router for Groups pages under <see cref="GroupsController"/> 
/// </summary>
public interface IGroupsRouter
{
    /// <summary>
    /// Routes to the SelectASchool page
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IActionResult GetSelectASchool(string groupName,
                                   List<EstablishmentLink> schools,
                                   Title title,
                                   List<ContentComponent> content,
                                   GroupsController controller,
                                   CancellationToken cancellationToken);


    /// <summary>
    /// Routes to the Groups Dashboard for a single school
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IActionResult GetSchoolDashboard(string schoolId,
                                     string schoolName,
                                     string groupName,
                                     List<ContentComponent> pageContent,
                                     GroupsController controller,
                                     CancellationToken cancellationToken);
}
