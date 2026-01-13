using System.Collections.Generic;
using System.Linq;
using cfg;
using QFramework;
using UnityEngine;
public interface IBoardEntitySystem : ISystem{
    BoardEntityMover Mover {get;}
    void RegisterEntity(BoardEntity entity, Vector2Int position);
    void UnregisterEntity(BoardEntity entity);
    BoardEntity GetEntity(string guid);
    void CreateEntity(string ID, Vector2Int position);
}

public class BoardEntitySystem : AbstractSystem, IBoardEntitySystem
{
    private Dictionary<string, BoardEntity> _entities = new Dictionary<string, BoardEntity>();

    public BoardEntityMover Mover{get; private set;}
    protected override void OnInit()
    {
        _entities = new Dictionary<string, BoardEntity>();
        Mover = new BoardEntityMover();
    }
    public BoardEntity GetEntity(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        _entities.TryGetValue(guid, out var entity);
        return entity;
    }
    // 创建实体
    public void CreateEntity(string ID, Vector2Int position)
    {
        // 1. 获取实体数据
        EntityData entityData = this.GetSystem<IDataSystem>().GetBoardEntityData(ID);
        // 2. 创建实体
        BoardEntity entity = new Entity(entityData, position);
        // 2.1 设置棋盘
        this.GetSystem<IBoardSystem>().SetCellInstance(position, entity.guid);
        // 3. 注册实体
        RegisterEntity(entity, position);
        this.SendEvent(new CreateEntityEvent(entity));
    }
    public void RegisterEntity(BoardEntity entity, Vector2Int position)
    {
        if (_entities.ContainsKey(entity.guid)) return;
        _entities.Add(entity.guid, entity);
        entity.position = position;
    }

    public void UnregisterEntity(BoardEntity entity)
    {
        if (!_entities.ContainsKey(entity.guid)) return;
        _entities.Remove(entity.guid);
        entity.position = new Vector2Int(-1, -1);
    }
}
