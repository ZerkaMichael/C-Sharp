using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Database of different types of conditions
public class ConditionsDB {
    //Initalize
    public static void Init() {
        foreach (var kvp in Conditions) {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition> {
        {
            ConditionID.psn,
            new Condition() {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Bot bot) => {
                    bot.UpdateHP(bot.MaxHp / 8);
                    bot.StatusChanges.Enqueue($"{bot.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition() {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Bot bot) => {
                    bot.UpdateHP(bot.MaxHp / 16);
                    bot.StatusChanges.Enqueue($"{bot.Base.Name} hurt itself due to being burned");
                }
            }
        },
        {
            ConditionID.par,
            new Condition() {
                Name = "Paralyzed",
                StartMessage = "has been Paralyzed",
                OnBeforeMove = (Bot bot) => {
                    if (Random.Range(1,5) == 1) {
                        bot.StatusChanges.Enqueue($"{bot.Base.Name}'s paralyzed and can't move");
                        return false;
                    } else {
                        return true;
                    }
                }
            }
        },
        {
            ConditionID.frz,
            new Condition() {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Bot bot) => {
                    if (Random.Range(1,5) == 1) {
                        bot.CureStatus();
                        bot.StatusChanges.Enqueue($"{bot.Base.Name} is no longer frozen");
                        return true;
                    } else {
                        return false; ;
                    }
                }
            }
        },
        {
            ConditionID.slp,
            new Condition() {
                Name = "Sleep",
                StartMessage = "is sleeping",
                OnStart = (Bot bot) => {
                    bot.StatusTime = Random.Range(1,4);
                },
                OnBeforeMove = (Bot bot) => {
                    if(bot.StatusTime <= 0) {
                        bot.CureStatus();
                        bot.StatusChanges.Enqueue($"{bot.Base.Name} woke up");
                        return true;
                    }
                    bot.StatusTime--;
                    bot.StatusChanges.Enqueue($"{bot.Base.Name} is sleeping. Turns left: {bot.StatusTime}");
                    return false; ;
                }
            }
        },
        { //Volatile Status Condition
            ConditionID.confusion,
            new Condition() {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Bot bot) => {
                    bot.VolatileStatusTime = Random.Range(1,5);
                },
                OnBeforeMove = (Bot bot) => {
                    if(bot.VolatileStatusTime <= 0) {
                        bot.CureVolatileStatus();
                        bot.StatusChanges.Enqueue($"{bot.Base.Name} snapped out of confusion");
                        return true;
                    }
                    bot.VolatileStatusTime--;
                    // 50% chance to do a move
                    if(Random.Range(1,3) == 1) {
                        return true;
                    }
                    //Hurt by confusion
                    bot.StatusChanges.Enqueue($"{bot.Base.Name} is confused");
                    bot.UpdateHP(bot.MaxHp / 8);
                    bot.StatusChanges.Enqueue($"{bot.Base.Name} is confused and hurt itself. Turns left: {bot.VolatileStatusTime}");
                    return false; 
                }
            }
        }
    };
}

//Key to dictionary
public enum ConditionID {
    none, psn, brn, slp, par, frz, 
    confusion
}