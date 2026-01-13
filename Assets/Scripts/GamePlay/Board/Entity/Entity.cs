
using System.Collections.Generic;
using cfg;
using UnityEngine;

public class Entity : BoardEntity
{
    public override string name => entityData.Name;
    public override BoardEntityType type => BoardEntityType.非食材;
    public override object data => entityData;
    public EntityData entityData;
    public Entity(EntityData entityData, Vector2Int position) : base()
    {
        this.entityData = entityData;
        this.position = position;
    }
    public override List<TooltipInfo> GetTooltipInfos()
    {
        List<TooltipInfo> tooltipInfos = new List<TooltipInfo>();
        tooltipInfos.Add(new TooltipInfo(entityData.Name));
        return tooltipInfos;
    }

    public override Sprite GetSprite() => Resources.Load<Sprite>("Sprites/" + entityData.Sprite);
}