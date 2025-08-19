using System.Collections;
using System.Reflection;
using Contentful.Core;
using Contentful.Core.Configuration;
using Contentful.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dfe.PlanTech.Infrastructure.SignIn.UnitTests;

//
// Summary:
//     Main class for interaction with the contentful delivery and preview APIs
public class HackedContentfulClient : ContentfulClientBase
{
    public IContentTypeResolver ContentTypeResolver { get; set; }

    public List<CrossSpaceResolutionSetting> CrossSpaceResolutionSettings { get; set; } = new List<CrossSpaceResolutionSetting>();

    public HackedContentfulClient(IContentTypeResolver contentTypeResolver)
    {
        ContentTypeResolver = contentTypeResolver;

        base.SerializerSettings.Converters.Add(new AssetJsonConverter());
        base.SerializerSettings.Converters.Add(new ContentJsonConverter());
        base.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
    }

    public ContentfulCollection<T> GetEntries<T>(string entriesJson)
    {
        JObject jObject = JObject.Parse(entriesJson);
        ReplaceMetaData(jObject);
        HashSet<string> processedIds = new HashSet<string>();
        JToken jToken = jObject.SelectToken("$.errors");
        if (jToken == null)
        {
            jObject.Add("errors", new JArray());
        }

        foreach (JObject item in jObject.SelectTokens("$.items[*]").OfType<JObject>())
        {
            ResolveLinks(jObject, item, processedIds, typeof(T));
        }

        List<JToken> list = jObject.SelectTokens("$.items[*]..fields").ToList();
        for (int num = list.Count - 1; num >= 0; num--)
        {
            JToken jToken2 = list[num];
            JContainer parent = jToken2.Parent.Parent;
            if ((parent["sys"]?["type"] == null || !(parent["sys"]["type"]?.ToString() != "Entry")) && !jToken2.Parent.Path.EndsWith(".fields.fields"))
            {
                ResolveContentTypes(parent);
                jToken2.Parent.Remove();
                parent.Add(jToken2.Children());
            }
        }

        var type = typeof(T).ToString();

        ContentfulCollection<T> contentfulCollection = jObject.ToObject<ContentfulCollection<T>>(base.Serializer);
        contentfulCollection.IncludedAssets = jObject.SelectTokens("$.includes.Asset[*]")?.Select((JToken t) => t.ToObject<Asset>(base.Serializer));
        contentfulCollection.IncludedEntries = jObject.SelectTokens("$.includes.Entry[*]")?.Select((JToken t) => t.ToObject<Entry<object>>(base.Serializer));
        return contentfulCollection;
    }

    private void ResolveContentTypes(JContainer container)
    {
        if (ContentTypeResolver == null || container["$type"] != null)
        {
            return;
        }

        string text = container["sys"]?["contentType"]?["sys"]?["id"]?.ToString();
        if (text != null)
        {
            Type type = ContentTypeResolver.Resolve(text);
            if (type != null)
            {
                container.AddFirst(new JProperty("$type", type.AssemblyQualifiedName));
            }
        }
    }

    private void ResolveLinks(JObject json, JObject entryToken, ISet<string> processedIds, Type type)
    {
        string text = ((JValue)entryToken.SelectToken("$.sys.id"))?.Value?.ToString();
        if (text == null)
        {
            text = ((JValue)entryToken.SelectToken("$.data.target.sys.id"))?.Value?.ToString();
        }

        if (text == null)
        {
            return;
        }

        ResolveContentTypes(entryToken);
        if (entryToken["$type"] != null)
        {
            type = Type.GetType(entryToken["$type"].Value<string>());
        }

        if (!processedIds.Contains(text))
        {
            entryToken.AddFirst(new JProperty("$id", new JValue(text)));
            processedIds.Add(text);
        }

        List<JToken> list = entryToken.SelectTokens("$.fields..sys").ToList();
        foreach (JToken item in list)
        {
            string propName = item.Path.Substring(item.Path.LastIndexOf(".fields.") + 8);
            propName = propName.Substring(0, propName.IndexOf("."));
            if (propName.IndexOf("[") > 0)
            {
                propName = propName.Substring(0, propName.IndexOf("["));
            }

            string text2 = "";
            string text3 = item["linkType"]?.ToString();
            if (item["type"]?.ToString() == "ResourceLink")
            {
                if (!CrossSpaceResolutionSettings.Any())
                {
                    continue;
                }

                text2 = ((JValue)item["urn"]).Value.ToString();
                text2 = ParseIdFromContentfulUrn(text2);
                text3 = (text3.Contains("Entry") ? "Entry" : "Asset");
            }
            else
            {
                text2 = ((JValue)item["id"]).Value.ToString();
            }

            JToken jToken = null;
            if (processedIds.Contains(text2))
            {
                jToken = new JObject { ["$ref"] = text2 };
            }
            else if (!string.IsNullOrEmpty(text3))
            {
                jToken = json.SelectTokens("$.includes." + text3 + "[?(@.sys.id=='" + text2 + "')]").FirstOrDefault();
                if (jToken == null)
                {
                    jToken = json.SelectTokens("$.items.[?(@.sys.id=='" + text2 + "')]").FirstOrDefault();
                }
            }

            JObject jObject = (JObject)item.Parent.Parent;
            if (jToken != null)
            {
                jObject.RemoveAll();
                jObject.Add(jToken.Children());
                PropertyInfo propertyInfo = null;
                propertyInfo = type?.GetRuntimeProperties().FirstOrDefault((PropertyInfo p) => p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase) || p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == propName);
                if ((!(propertyInfo == null) || !(text3?.ToString() != "Asset")) && !processedIds.Contains(text2))
                {
                    Type type2 = null;
                    type2 = propertyInfo?.PropertyType;
                    if (type2 != null && type2.IsArray)
                    {
                        type2 = type2.GetElementType();
                    }
                    else if (type2 != null && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type2.GetTypeInfo()) && type2.IsConstructedGenericType)
                    {
                        type2 = type2.GetTypeInfo().GenericTypeArguments[0];
                    }

                    ResolveLinks(json, jObject, processedIds, type2);
                }

                continue;
            }

            JToken jToken2 = json.SelectTokens("$.errors.[?(@.details.id=='" + text2 + "')]").FirstOrDefault();
            if (jToken2 != null)
            {
                JContainer jContainer = ((jObject.Parent is JProperty) ? jObject.Parent : jObject);
                jContainer.Remove();
                continue;
            }

            JContainer jContainer2 = ((jObject.Parent is JProperty) ? jObject.Parent : jObject);
            JToken jToken3 = json.SelectToken("$.errors");
            ContentfulError o = new ContentfulError
            {
                SystemProperties = new SystemProperties
                {
                    Id = "notResolvable",
                    Type = "error"
                },
                Details = new ContentfulErrorDetails
                {
                    Type = "Link",
                    LinkType = jContainer2.SelectToken("$..sys.linkType")?.Value<string>(),
                    Id = text2
                }
            };
            (jToken3 as JArray).Add(JObject.FromObject(o));
            jContainer2.Remove();
        }
    }

    private string ParseIdFromContentfulUrn(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        return s.Substring(s.LastIndexOf('/') + 1);
    }
}
