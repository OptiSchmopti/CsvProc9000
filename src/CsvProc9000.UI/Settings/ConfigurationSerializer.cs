using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;

namespace CsvProc9000.UI.Settings;

// c.f.: https://stackoverflow.com/a/73252837/8242470

/// <inheritdoc />
internal sealed class ConfigurationSerializer : IConfigurationSerializer
{
    /// <inheritdoc />
    public JsonNode Serialize(IConfiguration configuration)
    {
        var json = new JsonObject();

        var configurationChildren = configuration.GetChildren();
        foreach (var child in configurationChildren)
        {
            if (child.Path.EndsWith(":0"))
            {
                var array = new JsonArray();

                foreach (var arrayChild in configurationChildren)
                {
                    array.Add(Serialize(arrayChild));
                }

                return array;
            }

            json.Add(child.Key, Serialize(child));
        }

        if (json.Count != 0 || configuration is not IConfigurationSection section) return json;
        
        if (bool.TryParse(section.Value, out var boolean))
        {
            return JsonValue.Create(boolean);
        }
        
        if (decimal.TryParse(section.Value, out var real))
        {
            return JsonValue.Create(real);
        }
        
        if (long.TryParse(section.Value, out var integer))
        {
            return JsonValue.Create(integer);
        }

        return JsonValue.Create(section.Value);
    }
}
