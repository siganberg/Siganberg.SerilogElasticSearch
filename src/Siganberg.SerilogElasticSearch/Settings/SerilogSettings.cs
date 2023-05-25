using System.Collections.Generic;
using System.Linq;

namespace Siganberg.SerilogElasticSearch.Settings;

public class SerilogSettings
{
    public RequestLoggingOptions RequestLoggingOptions { get; set; }
    public const string KeyName = "Serilog";
}

public class RequestLoggingOptions
{
    private HashSet<string> _excludeNames = new();
    public bool IncludeResponseBody { get; set; } = false;
    public bool IncludeRequestHeaders { get; set; } = false;
    public HashSet<string> ExcludeHeaderNames {
        get => _excludeNames;
        set
        {
            _excludeNames = value.Select(a => a.ToLower()).ToHashSet();
        }
    } 
}       