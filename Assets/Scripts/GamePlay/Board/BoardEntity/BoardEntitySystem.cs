using System.Collections.Generic;
using System.Linq;
using QFramework;
using UnityEngine;
public interface IBoardEntitySystem : ISystem{
    void RegisterEntity(BoardEntity entity, Vector2Int position);
    void UnregisterEntity(BoardEntity entity);
    BoardEntity GetEntity(string guid);
}

public class BoardEntitySystem : AbstractSystem, IBoardEntitySystem
{
    private Dictionary<string, BoardEntity> _entities = new Dictionary<string, BoardEntity>();
    public BoardEntity GetEntity(string guid)
    {
        if (string.IsNullOrEmpty(guid)) return null;
        _entities.TryGetValue(guid, out var entity);
        return entity;
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

    protected override void OnInit()
    {
        _entities = new Dictionary<string, BoardEntity>();
    }
}
