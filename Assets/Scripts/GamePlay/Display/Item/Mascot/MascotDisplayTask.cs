using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;

public class MascotDisplayTask_随机获取若干 : IDisplayTask<MascotUIContext, DisplayMascotView>{
    private IItemInteractStrategy<MascotUIContext, DisplayMascotView> _strategy;
    private int _amount;
    public MascotDisplayTask_随机获取若干(IItemInteractStrategy<MascotUIContext, DisplayMascotView> strategy, int amount){
        _strategy = strategy;
        _amount = amount;
    }
    public IItemInteractStrategy<MascotUIContext, DisplayMascotView> GetStrategy() => _strategy;
    public IEnumerable<MascotUIContext> GetList(){
        IArchitecture architecture = GameArchitecture.Interface;
        Rng rng = architecture.GetSystem<IRngSystem>().GetSubRng<IMascotSystem>();

        List<MascotUIContext> mascotUIContexts = rng.PickMany<MascotData>(architecture.GetSystem<IDataSystem>().GetAllMascotData(), _amount)
            .Select(x => new MascotUIContext(new Mascot(x)))
            .ToList();

        foreach (var mascotUIContext in mascotUIContexts){
            mascotUIContext.SetPrice(rng.NextInt(4,6));
            mascotUIContext.SetAmount(1);
        }
        return mascotUIContexts;
    }
}