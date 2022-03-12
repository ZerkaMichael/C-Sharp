using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour {
    //Pick wild Bot in map area
    [SerializeField] List<Bot> wildBots;
    public Bot GetRandomWildBot() {
        var wildBot = wildBots[Random.Range(0, wildBots.Count)];
        wildBot.Init();
        return wildBot;
    }
}
