using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base file for new pokemon creation
[CreateAssetMenu(fileName = "Bot", menuName = "Bot/Create New Bot" )]
public class BotBase : ScriptableObject {
    //Pokemon Base
    [SerializeField]string name;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] BotType type1;
    [SerializeField] BotType type2;
    [SerializeField] List<LearnableMove> learnableMoves;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    public int GetExpForLevel(int level) {
        if(growthRate == GrowthRate.Fast) {
            return 4 * (level * level * level) / 5;
        } else if (growthRate == GrowthRate.Medium) {
            return level * level * level;
        } else {
            return -1;
        }
    }

    public string Name {
        get { return name; }
    }

    public string Description {
        get { return description; }
    }

    public int MaxHp {
        get { return maxHp; }
    }

    public int Attack {
        get { return attack; }
    }

    public int SpAttack {
        get { return spAttack; }
    }

    public int Defense {
        get { return defense; }
    }

    public BotType Type1 {
        get { return type1; }
    }

    public BotType Type2 {
        get { return type2; }
    }

    public int SpDefense {
        get { return spDefense; }
    }

    public int Speed {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves {
        get { return learnableMoves; }
    }

    public Sprite FrontSprite {
        get { return frontSprite; }
    }

    public Sprite BackSprite {
        get { return backSprite; }
    }

    public int ExpYield => expYield;
    public GrowthRate GrowthRate => growthRate;

}
      
[System.Serializable]

public class LearnableMove {
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base {
        get { return moveBase; }
    }

    public int Level {
        get { return level; }
    }
}

//Types
public enum BotType {
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

//Rate for leveling
public enum GrowthRate {
    Fast, Medium
}

//Types of stats
public enum Stat {
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    //moveAccuracy boosts
    Accuracy,
    Evasion
}

//Effectiveness chart
public class TypeChart {
    static float[][] chart = {
    //            NOR FIR WAT ELE GRA ICE FIG POI GRO FLY PSY BUG ROC GHO

         /*NOR*/ new float[]{ 1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,0.5f,0f},

         /*FIR*/ new float[]{ 1f,0.5f,0.5f,1f,2f,2f,1f,1f,1f,1f,1f,2f,0.5f,1f},

         /*WAT*/ new float[]{ 1f,2f,0.5f,1f,0.5f,1f,1f,1f,2f,1f,1f,1f,2f,1f},

         /*ELE*/ new float[]{ 1f,2f,0.5f,0.5f,1f,1f,1f,0f,2f,1f,1f,1f,1f,1f},

         /*GRA*/ new float[]{ 1f,0.5f,2f,1f,0.5f,1f,1f,0.5f,2f,0.5f,1f,0.5f,2f,1f},

         /*ICE*/ new float[]{ 1f,0.5f,0.5f,1f,2f,0.5f,1f,1f,2f,2f,1f,1f,1f,1f},

         /*FIG*/ new float[]{ 2f,1f,1f,1f,1f,2f,1f,0.5f,1f,0.5f,0.5f,0.5f,2f,0f},

         /*POI*/ new float[]{ 1f,1f,1f,1f,2f,1f,1f,0.5f,0.5f,1f,1f,1f,0.5f,0.5f},

         /*GRO*/ new float[]{ 1f,2f,1f,2f,0.5f,1f,1f,2f,0f,1f,0.5f,2f,1f,1f},

         /*FLY*/ new float[]{ 1f,1f,1f,0.5f,2f,1f,2f,1f,1f,1f,1f,2f,0.5f,1f},

         /*PSY*/ new float[]{ 1f,1f,1f,1f,1f,1f,2f,2f,1f,1f,0.5f,1f,1f,1f},

         /*BUG*/ new float[]{ 1f,0.5f,1f,1f,2f,1f,0.5f,0.5f,1f,0.5f,2f,1f,1f,0.5f},

         /*ROC*/ new float[]{ 1f,2f,1f,1f,1f,2f,0.5f,1f,0.5f,2f,1f,2f,1f,1f},

         /*GHO*/ new float[]{ 0f,1f,1f,1f,1f,1f,1f,1f,1f,1f,2f,1f,1f,2f},
    };


    public static float GetEffectiveness(BotType attackType, BotType defenseType) {
        if (attackType == BotType.None || defenseType == BotType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}