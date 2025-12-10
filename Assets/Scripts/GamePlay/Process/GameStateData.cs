using System;
using System.Collections.Generic;

[Serializable]
public class GameStateData{
    public int index;
    public string stateName;
    public List<GameStateData> subStates;
}