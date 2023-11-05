using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;

namespace CsvProc9000.UI.Wpf.Settings;

/// <summary>
///     Serializes <see cref="IConfiguration"/>s
/// </summary>
internal interface IConfigurationSerializer
{
    /// <summary>
    ///     Serializes the given <paramref name="configuration"/> into json
    /// </summary>
    /// <param name="configuration">The configuration that should be serialized</param>
    /// <returns>The resulting JSON</returns>
    JsonNode Serialize(IConfiguration configuration);
}
