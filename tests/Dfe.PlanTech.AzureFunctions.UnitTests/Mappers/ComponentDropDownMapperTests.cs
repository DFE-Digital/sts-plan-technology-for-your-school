using Dfe.PlanTech.AzureFunctions.Mappings;
using Dfe.PlanTech.Domain.Content.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Dfe.PlanTech.AzureFunctions.UnitTests.Mappers;

public class ComponentDropDownMapperTests : BaseMapperTests
{
  private const string DropdownTitle = "Dropdown title test";
  private const string DropdownId = "Dropdown Id";
  private readonly RichTextContent DropdownContent = new()
  {
    Data = new RichTextData()
    {
      Uri = "Test URI"
    },
    Marks = new(){
      new(){
        Type = "Bold"
      }
    },
    Content = new(){
      new(){
        Data = new(),
        Marks = new(),
        Value = "Inner text",
        NodeType = "paragraph",
        Content = new(){
          new(){
            Data = new(){
              Uri = "Inner uri"
            },
            Marks = new(){
              new() { Type = "Italics"}
            },
            Value = "Inner inner text",
            NodeType = "paragraph"
          }
        }
      }
    },
    NodeType = "document"
  };

  private readonly ComponentDropDownMapper _mapper;
  private readonly ILogger<JsonToDbMapper<ComponentDropDownDbEntity>> _logger;

  public ComponentDropDownMapperTests()
  {
    _logger = Substitute.For<ILogger<JsonToDbMapper<ComponentDropDownDbEntity>>>();
    _mapper = new ComponentDropDownMapper(MapperHelpers.CreateMockEntityRetriever(), MapperHelpers.CreateMockEntityUpdater(), _logger, JsonOptions);
  }

  [Fact]
  public void Mapper_Should_Map_Relationship()
  {
    var fields = new Dictionary<string, object?>()
    {
      ["title"] = WrapWithLocalisation(DropdownTitle),
      ["content"] = WrapWithLocalisation(DropdownContent),
    };

    var payload = CreatePayload(fields, DropdownId);

    var mapped = _mapper.ToEntity(payload);

    Assert.NotNull(mapped);

    var concrete = mapped;
    Assert.NotNull(concrete);

    Assert.Equal(DropdownId, concrete.Id);
    Assert.Equal(DropdownTitle, concrete.Title);
  }
}