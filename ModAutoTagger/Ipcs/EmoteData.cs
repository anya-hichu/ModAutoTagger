namespace ModAutoTagger.Ipcs;

public class EmoteData
{
    public string name = string.Empty;
    public string category = string.Empty;
    public string command = string.Empty;
    public string[] actionTimelineKeys = [];
    public string[] poseKeys = [];

    public EmoteData() { }
    public EmoteData(string command, string[] poseKeys)
    {
        this.command = command;
        this.poseKeys = poseKeys;
    }
}
