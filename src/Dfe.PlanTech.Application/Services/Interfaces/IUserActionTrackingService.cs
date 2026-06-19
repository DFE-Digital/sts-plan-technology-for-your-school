namespace Dfe.PlanTech.Application.Services.Interfaces;

public interface IUserActionTrackingService
{
    Task RecordActionAsync();

    Task RecordBannerViewAsync(string bannerId);

    Task<int> GetNumberOfTimesBannerViewedByUserAsync(string bannerId);
}
