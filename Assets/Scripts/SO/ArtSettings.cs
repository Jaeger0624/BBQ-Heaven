
using UnityEngine;

[CreateAssetMenu(fileName = "ArtSettings", menuName = "Settings/ArtSettings")]
public class ArtSettings : ScriptableObject
{
    public float NameSize = 54f;
    public float DescriptionSize = 36f;
    public SpriteReferenceSO StickSprites;
    public SpriteReferenceSO EnhancementSprites;
    public SpriteReferenceSO PlanetSprites;
    public SpriteReferenceSO CustomerSprites;
    public SpriteReferenceSO PicturesGuides;
}
