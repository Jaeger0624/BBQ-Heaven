using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager{
    private static EventManager _instance;
    public static EventManager Instance{
        get{
            if (_instance == null){
                _instance = new EventManager();
            }
            return _instance;
        }
    }
    private Dictionary<Type, List<Event>> _eventMap = new Dictionary<Type, List<Event>>();
    
    public void Register<T>(Action<T> callback, int priority = 0){
        if (!_eventMap.ContainsKey(typeof(T))){
            _eventMap[typeof(T)] = new List<Event>();
        }
        _eventMap[typeof(T)].Add(new Event(callback, priority));
    }
    public void Register<T>(Action callback, int priority = 0){
        if (!_eventMap.ContainsKey(typeof(T))){
            _eventMap[typeof(T)] = new List<Event>();
        }
        _eventMap[typeof(T)].Add(new Event(callback, priority));
    }
    public void Unregister<T>(Action<T> callback){
        if (_eventMap.ContainsKey(typeof(T))){
            int count = _eventMap[typeof(T)].RemoveAll(e => e.Equals(callback));
            if (count > 1){
                Debug.LogError($"尝试移除 {typeof(T)} 事件时，发现多个回调函数: {count}");
            }
        }
    }
    public void Unregister<T>(Action callback){
        if (_eventMap.ContainsKey(typeof(T))){
            int count = _eventMap[typeof(T)].RemoveAll(e => e.Equals(callback));
            if (count > 1){
                Debug.LogError($"尝试移除 {typeof(T)} 事件时，发现多个回调函数: {count}");
            }
        }
    }

    public void Trigger<T>(T eventData){
        if (eventData == null){
            Debug.LogError("事件数据为空");
            return;
        }
        if (!_eventMap.ContainsKey(typeof(T))){
            return;
        }
        if (_eventMap[typeof(T)].Count == 0){
            return;
        }

        // 从大到小排序
        _eventMap[typeof(T)].Sort((a, b) => b.Priority.CompareTo(a.Priority));

        for (int i = 0; i < _eventMap[typeof(T)].Count; i++){
            _eventMap[typeof(T)][i].Callback.DynamicInvoke(eventData);
        }
    }
}


public class Event{
    public Delegate Callback { get; set; } = null;
    public int Priority { get; set; } = 0;
    public Event(Delegate callback, int priority){
        Callback = callback;
        Priority = priority;
    }
    public bool Equals(Delegate callback){
        return Callback == callback;
    }
}