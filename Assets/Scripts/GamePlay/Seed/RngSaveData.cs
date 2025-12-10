using System;
using System.Collections.Generic;
using Sirenix.Serialization;

[Serializable]
public class RngSaveData{
    public int MainSeed;
    [OdinSerialize]
    public Dictionary<string, int> SubRngCallCounts;
}