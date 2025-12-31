using System.Collections.Generic;
using System.Linq;
using cfg;

/// <summary>
/// 瞬间遭遇 - 实例层
/// </summary>
public class InstantEncounter{
    public string Name => data.Name;
    public string Description => data.Description;
    public readonly InstantEncounterData data;
    public readonly List<OptionData> options;
    public InstantEncounter(InstantEncounterData data ,List<OptionData> options){
        this.data = data;
        this.options = options;
    }
}