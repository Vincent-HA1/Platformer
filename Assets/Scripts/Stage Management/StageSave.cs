using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
