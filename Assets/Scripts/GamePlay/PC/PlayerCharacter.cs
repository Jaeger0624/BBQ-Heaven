using System;
using cfg;
using QFramework;
[Serializable]
public partial class PlayerCharacter : ICanGetSystem{
    public string ID => data.ID;
    public string name;
    public string description;
    public readonly PCData data;
    public PlayerCharacter(PCData data){
        this.data = data;
    }

    public IArchitecture GetArchitecture() => GameArchitecture.Interface;

}