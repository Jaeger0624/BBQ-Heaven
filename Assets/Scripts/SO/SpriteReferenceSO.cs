using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CreateAssetMenu(fileName = "SpriteReferenceSO", menuName = "SpriteReferenceSO")]
public class SpriteReferenceSO : ScriptableObject
{
    public List<Sprite> sprites;
    public Sprite GetSprite(string name){
        return sprites.Find(sprite => sprite.name == name);
    }


    [Button("Load Sprites")]
    public void LoadSprites(string prefix){
        // 寻找相同名字的Sprite，并添加到sprites列表中
        this.sprites = new List<Sprite>();
        List<Sprite> allSprites = Resources.LoadAll<Sprite>("Art").ToList();
        foreach (Sprite sprite in allSprites){
            if (sprite.name.StartsWith(prefix)) this.sprites.Add(sprite);
        }
    }
}