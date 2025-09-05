using System.Collections.Generic;

[System.Serializable]
public class StageSave
{
    public string stageName;
    public List<int> bigCoinsFound;

    public StageSave(string stageName, List<int> bigCoinsFound)
    {
        this.stageName = stageName;
        this.bigCoinsFound = bigCoinsFound;
    }
}
