using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//States the game will be in during the battle
public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}

public class BattleSystem : MonoBehaviour {
    //Player Data
    [SerializeField] BattleUnit playerUnit;

    //Enemy Data
    [SerializeField] BattleUnit enemyUnit;

    //Screen Data
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    //Event variable
    public event Action<bool> OnBattleOver;

    //Control variables
    BattleState state;
    BattleState? prevState; //? to make nullable
    int currentAction;
    int currentMove;
    int currentMember;

    //Pokemon party and wild enemy variables
    PokemonParty playerParty;
    Pokemon wildPokemon;

    //Runs the setup of the battle on start
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) {
        //Sets pokemon and wild pokemon variables
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        //Setup battle coroutine -- runs the Ienumerator
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        //Player setup
        playerUnit.Setup(playerParty.GetHealthyPokemon());

        //Enemy setup
        enemyUnit.Setup(wildPokemon);

        //Party
        partyScreen.Init();

        //Dialog for moves
        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);


        //Text for encountering an enemy and wait time until player can use move selector
        yield return dialogBox.TypeDialog($"A Wild {enemyUnit.Pokemon.Base.Name} appeared.");
        //First action
        ActionSelection();
    }

    //Set state to battle over and call event -- requires input of whether battle is won or over
    void BattleOver(bool won) {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won); 
    }

    //Start of players turn (Action select)
    void ActionSelection() {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an Action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen() {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    //Move selection
    void MoveSelection() {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //Handles the turn system
    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RunningTurn;
        if(playerAction == BattleAction.Move) {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();
            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;
            //Decide who goes first - based on speed
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority) {
                playerGoesFirst = false;
            } else if (enemyMovePriority == playerMovePriority) {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondPokemon = secondUnit.Pokemon;
            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) { yield break; }
            //Second Turn
            if (secondPokemon.HP > 0) {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) { yield break; }
            }
        } else {
            if(playerAction == BattleAction.SwitchPokemon) {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            //Enemy turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) { yield break; }
        }
        if(state != BattleState.BattleOver) {
            ActionSelection();
        }
    }

    //Move logic
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {
        //Before move -- Stop if can't attack
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove) {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        //Decrease pp on attack
        move.PP--;

        //Hit check
        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)) {
            //Player Dialog and attack animation
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            //Enemy Damage animation
            targetUnit.PlayHitAnimation();

            //Move type and display status changes text
            if (move.Base.Category == MoveCategory.Status) {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon,move.Base.Target);
            } else {
                //Handle updating and drawing faint, damagedetails, and enemyHP
                var damageDetails = targetUnit.Pokemon.TakeDamge(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                //Update and draw damageDetails
                yield return ShowDamageDetails(damageDetails);
            }
            //Run secondary effects
            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0) {
                foreach (var secondary in move.Base.Secondaries) {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if(rnd <= secondary.Chance) {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }
            //Faint check
            if (targetUnit.Pokemon.HP <= 0) {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} has Fainted");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);
                CheckForBattleOver(targetUnit);
            }
        } else { 
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed"); 
        }
    }

    //Move effections decisions
    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget) {
        //Stat Boost
        if (effects.Boosts != null) {
            if (moveTarget == MoveTarget.Self) {
                source.ApplyBoosts(effects.Boosts);
            } else {
                target.ApplyBoosts(effects.Boosts);
            }
        }        
       //Status Condition
        if (effects.Status != ConditionID.none) {
            target.SetStatus(effects.Status);
        }
        //Volitale status conditions
        if (effects.VolatileStatus != ConditionID.none) {
            target.SetVolatileStatus(effects.VolatileStatus);
        }
            yield return ShowStatusChanges(source);
            yield return ShowStatusChanges(target);
    }

    //After turn
    IEnumerator RunAfterTurn(BattleUnit sourceUnit) {
        if (state == BattleState.BattleOver) { yield break; }
        //Wait for state to return to running turn for both units
        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        //After turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        //After turn faint check
        if (sourceUnit.Pokemon.HP <= 0) {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} has Fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver(sourceUnit);
        }
    }

    //Accuracy check
    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target) {
        if (move.Base.AlwaysHits) return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        var boostValues = new float[] { 1f, 4f/3f, 5f/3f, 2f,7f/3f, 8f/3f, 3f };

        if (accuracy > 0) {
            moveAccuracy *= boostValues[accuracy];
        } else {
            moveAccuracy /= boostValues[-accuracy];
        }

        if (evasion > 0) {
            moveAccuracy /= boostValues[evasion];
        } else {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    //Show status changes text
    IEnumerator ShowStatusChanges(Pokemon pokemon) {
        while(pokemon.StatusChanges.Count > 0) {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    //Call to see if battle is over after faint
    void CheckForBattleOver(BattleUnit faintedUnit) {
        if (faintedUnit.IsPlayerUnit) {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null) {
                OpenPartyScreen();
            } else {
                BattleOver(false);
            }
        } else {
            BattleOver(true);
        }
    }


    //Update and draw damage details
    IEnumerator ShowDamageDetails(DamageDetails damageDetails) {
        if (damageDetails.Critical > 1f) {
            yield return dialogBox.TypeDialog("A critical hit!");
        } else if (damageDetails.Type > 1f) {
           yield return dialogBox.TypeDialog("It's super effective!");
        } else if (damageDetails.Type < 1f) {
            yield return dialogBox.TypeDialog("It's not very effective!");
        }
    }

    //Handles updating State switches
    public void HandleUpdate() {
        if (state == BattleState.ActionSelection) {
            HandleActionSelection();
        }else if (state == BattleState.MoveSelection) {
            HandleMoveSelection();
        } else if (state == BattleState.PartyScreen) {
            HandlePartySelection();
        }
    }

    //Action selection key handling
    void HandleActionSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
                ++currentAction;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                --currentAction;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                currentAction += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                currentAction -= 2;
        }

        //Set limit
        currentAction = Mathf.Clamp(currentAction, 0, 3);

        //Update Screen
        dialogBox.UpdateActionSelection(currentAction);
        //
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (currentAction == 0) {
                //Fight
                MoveSelection();
            } else if (currentAction == 1) {
                //Bag
            } else if (currentAction == 2) {
                //Pokemon
                prevState = state;
                OpenPartyScreen();
            } else if (currentAction == 3) {
                //Run
            }
        }
    }

    //Move selection key handling
    void HandleMoveSelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMove;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMove;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMove += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMove -= 2;
        }

        //Set limit
        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        //Update screen
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        //Select move -- Z for select || X for back
        if (Input.GetKeyDown(KeyCode.Z)) {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) { return; }
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        } else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection() {
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            ++currentMember;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            --currentMember;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentMember += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentMember -= 2;
        }

        //Set limit
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        //Update screen
        partyScreen.UpdateMemberSelection(currentMember);

        //Select action(new pokemon)
        if (Input.GetKeyDown(KeyCode.Z)) {
            var selectedMember = playerParty.Pokemons[currentMember];
            //Validity check
            if (selectedMember.HP <= 0) { partyScreen.SetMessageText("You can't send out a dead pokemon"); return; }
            if (selectedMember == playerUnit.Pokemon) { partyScreen.SetMessageText("That Pokemon is already active"); return; }
            //Switch and update screen
            partyScreen.gameObject.SetActive(false);
            if (prevState == BattleState.ActionSelection) {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            } else {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        } else if (Input.GetKeyDown(KeyCode.X)) {
            //Go back
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }
    //Switch pokemon
    IEnumerator SwitchPokemon(Pokemon newPokemon) {
        if (playerUnit.Pokemon.HP > 0) {
            yield return dialogBox.TypeDialog($"Return {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"{newPokemon.Base.Name}!");
        state = BattleState.RunningTurn;
    }
}
