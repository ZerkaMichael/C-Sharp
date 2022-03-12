using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The base file for new move creation
[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create New Move")]
public class MoveBase : ScriptableObject {
    //Variables
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;
    [SerializeField] BotType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int giveHP;
    [SerializeField] bool alwaysHits;
    [SerializeField] int pp;
    [SerializeField] int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;


    //Property get and set
    public string Name {
        get { return name; }
    }
    public string Description {
        get { return description; }
    }
    public BotType Type {
        get { return type; }
    }
    public int Power {
        get { return power; }
    }
    public int Accuracy {
        get { return accuracy; }
    }
    public int GiveHP {
        get { return giveHP; }
    }
    public bool AlwaysHits {
        get { return alwaysHits; }
    }
    public int PP {
        get { return pp; }
    }    
    public int Priority {
        get { return priority; }
    }
    public MoveCategory Category {
        get { return category; }
    }
    public MoveEffects Effects {
        get { return effects; }
    }
    public List<SecondaryEffects> Secondaries {
        get { return secondaries; }
    }
    public MoveTarget Target {
        get { return target; }
    }
}

//Types of move effects -- Seralize class so it shows up in the inspector
[System.Serializable]
public class MoveEffects {
    //Variables
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID volatileStatus;

    //Properties
    public List<StatBoost> Boosts {
        get { return boosts; }
    }

    public ConditionID Status {
        get { return status; }
    }  
    public ConditionID VolatileStatus {
        get { return volatileStatus; }
    }
}
[System.Serializable]
public class SecondaryEffects : MoveEffects {
    [SerializeField] int chance;
    [SerializeField] MoveTarget target;

    public int Chance {
        get { return chance; }
    }

    public MoveTarget Target {
        get { return target; }
    }
}


//Class to hold variable data -- Seralize class so it shows up in the inspector
[System.Serializable]
public class StatBoost {
    public Stat stat;
    public int boost;
}
//Types of moves
public enum MoveCategory {
    Physical, Special, Heal, Status
}
//Possible targets
public enum MoveTarget {
    Foe, Self
}