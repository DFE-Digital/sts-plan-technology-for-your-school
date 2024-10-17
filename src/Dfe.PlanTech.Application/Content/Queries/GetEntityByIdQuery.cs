using AutoMapper;
using Dfe.PlanTech.Application.Core;
using Dfe.PlanTech.Application.Persistence.Interfaces;
using Dfe.PlanTech.Domain.Content.Interfaces;
using Dfe.PlanTech.Domain.Content.Models;
using Dfe.PlanTech.Domain.Questionnaire.Models;
using Microsoft.Extensions.Logging;

namespace Dfe.PlanTech.Application.Content.Queries;

/// <summary>
/// Retrieves entities from the CMS
/// </summary>
public class GetEntityByIdQuery : ContentRetriever, IGetEntityByIdQuery
{
    private const string ExceptionMessageContentful = "Error fetching Entity from Contentful";
    private const string ExceptionMessageDatabase = "Error fetching Entity from Database";

    private readonly ICmsDbContext _db;
    private readonly ILogger<GetEntityByIdQuery> _logger;
    private readonly IMapper _mapper;

    public GetEntityByIdQuery(ICmsDbContext db, ILogger<GetEntityByIdQuery> logger, IContentRepository repository,
        IMapper mapper) : base(repository)
    {
        _db = db;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Question?> GetQuestion(string questionId, CancellationToken cancellationToken = default)
        => await GetEntity<QuestionDbEntity, Question>(questionId, cancellationToken);


    private async Task<TContent?> GetEntity<TEntity, TContent>(string contentId,
        CancellationToken cancellationToken = default)
        where TEntity : ContentComponentDbEntity
        where TContent : ContentComponent
    {
        // var dbEntity = await GetFromDatabase<TEntity>(contentId, cancellationToken);
        //
        // if (dbEntity != null)
        //     return _mapper.Map<TContent>(dbEntity);

        return await GetFromContentful<TContent>(contentId, cancellationToken);
    }

    private async Task<TEntity?> GetFromDatabase<TEntity>(string contentId, CancellationToken cancellationToken)
        where TEntity : ContentComponentDbEntity
    {
        try
        {
            return await _db.GetEntityById<TEntity>(contentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageDatabase);
            return null;
        }
    }

    private async Task<TContent?> GetFromContentful<TContent>(string contentId, CancellationToken cancellationToken)
        where TContent : ContentComponent
    {
        try
        {
            var entities = await repository.GetEntities<TContent>(cancellationToken);
            return entities.FirstOrDefault(entity => entity.Sys.Id == contentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ExceptionMessageContentful);
            return null;
        }
    }
}
