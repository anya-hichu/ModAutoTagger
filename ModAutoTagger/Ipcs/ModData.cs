using System.Collections.Generic;

namespace ModAutoTagger.Ipcs;

public class ModData
{
    public string dir = string.Empty;
    public string name = string.Empty;
    public string path = string.Empty;
    public string[] localTags = [];
    public Dictionary<string, object> settings = [];
}
