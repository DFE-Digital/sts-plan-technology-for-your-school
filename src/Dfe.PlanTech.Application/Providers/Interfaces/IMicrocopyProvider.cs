using Dfe.PlanTech.Core.Models;

namespace Dfe.PlanTech.Application.Providers.Interfaces;

public interface IMicrocopyProvider
{
    Task<Dictionary<string, MicrocopyModel>> CreateRecordsAsync();
    Task<string> GetTextByKeyAsync(
        string key,
        Dictionary<string, string>? dynamicValues = null
        );
    Task<MicrocopyModel?> GetRecordByKeyAsync(string key);
}

