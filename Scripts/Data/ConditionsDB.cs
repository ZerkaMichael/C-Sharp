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
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition() {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) => {
                    pokemon.UpdateHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} hurt itself due to being burned");
                }
            }
        },
        {
            ConditionID.par,
            new Condition() {
                Name = "Paralyzed",
                StartMessage = "has been Paralyzed",
                OnBeforeMove = (Pokemon pokemon) => {
                    if (Random.Range(1,5) == 1) {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name}'s paralyzed and can't move");
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
                OnBeforeMove = (Pokemon pokemon) => {
                    if (Random.Range(1,5) == 1) {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer frozen");
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
                OnStart = (Pokemon pokemon) => {
                    pokemon.StatusTime = Random.Range(1,4);
                },
                OnBeforeMove = (Pokemon pokemon) => {
                    if(pokemon.StatusTime <= 0) {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is sleeping. Turns left: {pokemon.StatusTime}");
                    return false; ;
                }
            }
        },
        { //Volatile Status Condition
            ConditionID.confusion,
            new Condition() {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Pokemon pokemon) => {
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                },
                OnBeforeMove = (Pokemon pokemon) => {
                    if(pokemon.VolatileStatusTime <= 0) {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} snapped out of confusion");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;
                    // 50% chance to do a move
                    if(Random.Range(1,3) == 1) {
                        return true;
                    }
                    //Hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused");
                    pokemon.UpdateHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused and hurt itself. Turns left: {pokemon.VolatileStatusTime}");
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