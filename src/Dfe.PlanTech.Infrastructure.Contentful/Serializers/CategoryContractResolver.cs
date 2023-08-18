using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

public class DependencyInjectionContractResolver : DefaultContractResolver
{
    private readonly IServiceProvider _serviceProvider;

    public DependencyInjectionContractResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var service = _serviceProvider.GetService(objectType);
        if (service == null)
        {
            return base.CreateObjectContract(objectType);
        }

        JsonObjectContract contract = base.CreateObjectContract(objectType);
        contract.DefaultCreator = () => service;

        return contract;
    }
}