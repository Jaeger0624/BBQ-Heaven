using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteReferenceSO", menuName = "SpriteReferenceSO")]
public class SpriteReferenceSO : ScriptableObject
{
    public List<Sprite> sprites;
    public Sprite GetSprite(string name){
        return sprites.Find(sprite => sprite.name == name);
    }
}
