using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotParty : MonoBehaviour {
    //Available bots in party
    [SerializeField] List<Bot> bots;

    public List<Bot> Bots {
        get {
            return bots;
        }
    }
    //Add Bots to party
    private void Start() {
        foreach (var bot in bots) {
            bot.Init();
        }
    }

    //Get first alive bot for battle
    //Where is a loop that will go through all the bots in party with HP greater than 0 -- Picks the first one
    public Bot GetHealthyBot() {
        return bots.Where(x => x.HP > 0).FirstOrDefault();
    }
}
