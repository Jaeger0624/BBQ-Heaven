using System.Collections.Generic;
using System.Linq;
using QFramework;

public class FoodCollectionProvider : ICollectionDataProvider, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public CollectionType Type => CollectionType.Food;
    public List<CollectionItemDefinition> GetAllItems()
    {
        return this.GetSystem<IDataSystem>().GetAllFoodData().Select(item => new CollectionItemDefinition{
            ID = item.ID,
            DefaultUnlocked = false
        }).ToList();
    }
}

public class StickCollectionProvider : ICollectionDataProvider, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public CollectionType Type => CollectionType.Stick;
    public List<CollectionItemDefinition> GetAllItems()
    {
        return this.GetSystem<IDataSystem>().GetAllStickData().Select(item => new CollectionItemDefinition{
            ID = item.ID,
            DefaultUnlocked = false
        }).ToList();
    }
}

public class CardCollectionProvider : ICollectionDataProvider, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public CollectionType Type => CollectionType.Card;
    public List<CollectionItemDefinition> GetAllItems()
    {
        return this.GetSystem<IDataSystem>().GetAllCardData().Select(item => new CollectionItemDefinition{
            ID = item.ID,
            DefaultUnlocked = false
        }).ToList();
    }
}

public class RecipeCollectionProvider : ICollectionDataProvider, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public CollectionType Type => CollectionType.Recipe;
    public List<CollectionItemDefinition> GetAllItems()
    {
        return this.GetSystem<IDataSystem>().GetAllRecipeData().Select(item => new CollectionItemDefinition{
            ID = item.ID,
            DefaultUnlocked = false
        }).ToList();
    }
}

public class MascotCollectionProvider : ICollectionDataProvider, ICanGetSystem{
    public IArchitecture GetArchitecture() => GameArchitecture.Interface;
    public CollectionType Type => CollectionType.Mascot;
    public List<CollectionItemDefinition> GetAllItems()
    {
        return this.GetSystem<IDataSystem>().GetAllMascotData().Select(item => new CollectionItemDefinition{
            ID = item.ID,
            DefaultUnlocked = false
        }).ToList();
    }
}
