using UnityEngine;
public abstract class ValueSO<T> : ScriptableObject
{
    public T value;
    public T GetValue() => value;
    public void SetValue(T value) => this.value = value;
}