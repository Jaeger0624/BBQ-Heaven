using QFramework;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationController : MonoBehaviour, IController
{
    ITextSpawner textSpawner;

    // MM
    private void Awake()
    {
        textSpawner = GetComponent<ITextSpawner>();
    }
    void OnEnable()
    {
        this.RegisterEvent<SpawnTextEvent>(OnSpawnTextEvent);
    }
    void OnDisable()
    {
        this.UnRegisterEvent<SpawnTextEvent>(OnSpawnTextEvent);
    }
    [Button]
    public void TestSpawnText(){

    }
    private void OnSpawnTextEvent(SpawnTextEvent evt){
        if (evt.lifetime.HasValue){
            textSpawner.Spawn(evt.text, evt.size, evt.position, evt.color.color, evt.lifetime.Value, true);
            return;
        }

        if (evt.color.useColor){
            textSpawner.Spawn(evt.text, evt.size, evt.position, evt.color.color, null, true);
        } else {
            textSpawner.Spawn(evt.text, evt.size, evt.position, Color.white, null, true);
        }
    }
    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}



public class SpawnTextEvent : AbstractEvent{
    public string text;
    public float size;
    public (Color color, bool useColor) color;
    public Vector3 position;
    public float? lifetime = null;
    public SpawnTextEvent(string text, float size, Color color, Vector3 position){
        this.text = text;
        this.size = size;
        this.color = (color, true);
        this.position = position;
    }

    public SpawnTextEvent(string text, float size, Color color, Vector3 position, float lifetime){
        this.text = text;
        this.size = size;
        this.color = (color, true);
        this.position = position;
        this.lifetime = lifetime;
    }

    public SpawnTextEvent(string text, float size, Vector3 position){
        this.text = text;
        this.size = size;
        this.position = position;
        this.color = (Color.white, false);
    }
}
