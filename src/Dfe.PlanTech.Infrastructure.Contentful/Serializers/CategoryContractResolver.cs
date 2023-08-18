using System;
using System.Reflection;
using Dfe.PlanTech.Domain.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Interfaces;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class CategoryContractResolver : DefaultContractResolver
{
    private readonly IServiceProvider _serviceProvider;

    public CategoryContractResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (property.PropertyName == nameof(ICategory.Query))
        {
            property.ValueProvider = new QueryValueProvider(_serviceProvider);
        }
        else if (property.PropertyName == nameof(ICategory.Logger))
        {
            property.ValueProvider = new LoggerValueProvider(_serviceProvider);
        }

        return property;
    }
}

public class QueryValueProvider : IValueProvider
{
    private readonly IServiceProvider _serviceProvider;

    public QueryValueProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object GetValue(object target)
    {
        return _serviceProvider.GetRequiredService<IGetSubmissionStatusesQuery>();
    }

    public void SetValue(object target, object value)
    {
        
    }
}

public class LoggerValueProvider : IValueProvider
{
    private readonly IServiceProvider _serviceProvider;

    public LoggerValueProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object GetValue(object target)
    {
        return _serviceProvider.GetRequiredService<ILogger<Category>>();
    }

    public void SetValue(object target, object value)
    {
    }
}