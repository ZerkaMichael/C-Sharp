using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//States the game will be in during the battle
public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}
public enum BattleAction { Move, SwitchBot, UseItem, Run}

public class BattleSystem : MonoBehaviour {
    //Player Data
    [SerializeField] BattleUnit playerUnit;

    //Enemy Data
    [SerializeField] BattleUnit enemyUnit;

    //Screen Data
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    //Event variable
    public event Action<bool> OnBattleOver;

    //Control variables
    BattleState state;
    BattleState? prevState; //? to make nullable
    int currentAction;
    int currentMove;
    int currentMember;
    bool isTrainerBattle = false;
    bool aboutToUseChoice = true;
    int escapeAttempts;

    //Pokemon party and enemy variables
    BotParty playerParty;
    BotParty trainerParty;
    Bot wildBot;
    PlayerController player;
    TrainerController trainer;

    //Runs the setup of the battle on start
    public void StartBattle(BotParty playerParty, Bot wildPokemon) {
        //Sets pokemon and wild pokemon variables
        this.playerParty = playerParty;
        this.wildBot = wildPokemon;
        //Setup battle coroutine -- runs the Ienumerator
        StartCoroutine(SetupBattle());
    }
    public void StartTrainerBattle(BotParty playerParty, BotParty trainerParty) {
        //Sets player and trainer parties
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        //Setup battle coroutine -- runs the Ienumerator
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() {
        playerUnit.Clear();
        enemyUnit.Clear();
        if (!isTrainerBattle) {
            //Player setup
            playerUnit.Setup(playerParty.GetHealthyBot());
            //Enemy setup
            enemyUnit.Setup(wildBot);
            //Dialog for moves
            dialogBox.SetMoveNames(playerUnit.Bot.Moves);
            //Text for encountering an enemy and wait time until player can use move selector
            yield return dialogBox.TypeDialog($"A Wild {enemyUnit.Bot.Base.Name} appeared.");
        } else {
            //Trainer Battle
            //Show Sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            //Send out first Bot | Trainer -> Player
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyBot = trainerParty.GetHealthyBot();
            enemyUnit.Setup(enemyBot);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyBot.Base.Name}!");

            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerBot = playerParty.GetHealthyBot();
            playerUnit.Setup(playerBot);
            yield return dialogBox.TypeDialog($"{player.Name} sent out {playerBot.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Bot.Moves);
        }
        //Party
        partyScreen.Init();
        escapeAttempts = 0;
        //First action
        ActionSelection();
    }

    //Set state to battle over and call event -- requires input of whether battle is won or over
    void BattleOver(bool won) {
        state = BattleState.BattleOver;
        playerParty.Bots.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    //Start of players turn (Action select)
    void ActionSelection() {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an Action");
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen() {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Bots);
        partyScreen.gameObject.SetActive(true);
    }

    //Move selection
    void MoveSelection() {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //Handles switching between trainer faints
    IEnumerator AboutToUse(Bot newBot) {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newBot.Base.Name}! Do you want to switch?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    //Handles the turn system
    IEnumerator RunTurns(BattleAction playerAction) {
        state = BattleState.RunningTurn;
        if (playerAction == BattleAction.Move) {
            playerUnit.Bot.CurrentMove = playerUnit.Bot.Moves[currentMove];
            enemyUnit.Bot.CurrentMove = enemyUnit.Bot.GetRandomMove();
            int playerMovePriority = playerUnit.Bot.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Bot.CurrentMove.Base.Priority;
            //Decide who goes first - based on speed
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority) {
                playerGoesFirst = false;
            } else if (enemyMovePriority == playerMovePriority) {
                playerGoesFirst = playerUnit.Bot.Speed >= enemyUnit.Bot.Speed;
            }
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondBot = secondUnit.Bot;
            //First Turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Bot.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) { yield break; }
            //Second Turn
            if (secondBot.HP > 0) {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Bot.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) { yield break; }
            }
        } else {
            if (playerAction == BattleAction.SwitchBot) {
                var selectedBot = playerParty.Bots[currentMember];
                state = BattleState.Busy;
                yield return SwitchBot(selectedBot);
            } else if (playerAction == BattleAction.Run) {
                yield return TryToEscape();
            }
            //Enemy turn
            var enemyMove = enemyUnit.Bot.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) { yield break; }
        }
        if (state != BattleState.BattleOver) {
            ActionSelection();
        }
    }

    //Move logic
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) {
        //Before move -- Stop if can't attack
        bool canRunMove = sourceUnit.Bot.OnBeforeMove();
        if (!canRunMove) {
            yield return ShowStatusChanges(sourceUnit.Bot);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Bot);
        //Decrease pp on attack
        move.PP--;

        //Hit check
        if (CheckIfMoveHits(move, sourceUnit.Bot, targetUnit.Bot)) {
            //Player Dialog and attack animation
            yield return dialogBox.TypeDialog($"{sourceUnit.Bot.Base.Name} used {move.Base.Name}");
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);

            //target Damage animation
            targetUnit.PlayHitAnimation();

            //Move type and display status changes text
            if (move.Base.Category == MoveCategory.Status) {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Bot, targetUnit.Bot, move.Base.Target);
            } else if (move.Base.Category == MoveCategory.Heal) {
                //GiveHP if enabled
                int giveHP = move.Base.GiveHP;
                var damageDetails = sourceUnit.Bot.Heal(move, sourceUnit.Bot);
                if (giveHP < 0) {
                    yield return ShowDamageDetails(damageDetails);
                    yield return sourceUnit.Hud.UpdateHP();
                }
            } else {
                //Handle updating and drawing faint, damagedetails, and enemyHP
                var damageDetails = targetUnit.Bot.TakeDamge(move, sourceUnit.Bot);
                yield return targetUnit.Hud.UpdateHP();
                //Update and draw damageDetails
                yield return ShowDamageDetails(damageDetails);
            }
            //Run secondary effects
            if (move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Bot.HP > 0) {
                foreach (var secondary in move.Base.Secondaries) {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance) {
                        yield return RunMoveEffects(secondary, sourceUnit.Bot, targetUnit.Bot, secondary.Target);
                    }
                }
            }
            //Faint check
            if (targetUnit.Bot.HP <= 0) {
                yield return HandlePokemonFainted(targetUnit);
            }
        } else {
            yield return dialogBox.TypeDialog($"{sourceUnit.Bot.Base.Name}'s attack missed");
        }
    }

    //Move effections decisions
    IEnumerator RunMoveEffects(MoveEffects effects, Bot source, Bot target, MoveTarget moveTarget) {
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
        sourceUnit.Bot.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Bot);
        yield return sourceUnit.Hud.UpdateHP();
        //After turn faint check
        if (sourceUnit.Bot.HP <= 0) {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    //Accuracy check
    bool CheckIfMoveHits(Move move, Bot source, Bot target) {
        if (move.Base.AlwaysHits) return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];
        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

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
    IEnumerator ShowStatusChanges(Bot pokemon) {
        while (pokemon.StatusChanges.Count > 0) {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit) {
        yield return dialogBox.TypeDialog($"{faintedUnit.Bot.Base.Name} has Fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        //Handle giving XP
        if (!faintedUnit.IsPlayerUnit) {
            int expYield = faintedUnit.Bot.Base.ExpYield;
            int enemyLevel = faintedUnit.Bot.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;
            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Bot.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Bot.Base.Name} has gained {expGain} exp!");
            yield return playerUnit.Hud.SetExpSmooth();


            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
    }

    //Call to see if battle is over after faint
    void CheckForBattleOver(BattleUnit faintedUnit) {
        if (faintedUnit.IsPlayerUnit) {
            var nextPokemon = playerParty.GetHealthyBot();
            if (nextPokemon != null) {
                OpenPartyScreen();
            } else {
                BattleOver(false);
            }
        } else {
            if (!isTrainerBattle) {
                BattleOver(true);
            } else {
                var nextBot = trainerParty.GetHealthyBot();
                if (nextBot != null) {
                    StartCoroutine(AboutToUse(nextBot));
                } else {
                    BattleOver(true);
                }
            }
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
        switch (state) {
            case BattleState.ActionSelection:
                HandleActionSelection();
                break;
            case BattleState.MoveSelection:
                HandleMoveSelection();
                break;
            case BattleState.PartyScreen:
                HandlePartySelection();
                break;
            case BattleState.AboutToUse:
                HandleAboutToUse();
                break;
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
        //Action selection handler
        if (Input.GetKeyDown(KeyCode.Z)) {
            switch (currentAction) {
                case 0:
                    //Fight
                    MoveSelection();
                    break;
                case 1:
                    //Bag
                    break;
                case 2:
                    //Pokemon
                    prevState = state;
                    OpenPartyScreen();
                    break;
                case 3:
                    //Run
                    StartCoroutine(RunTurns(BattleAction.Run));
                    break;
            }
        }
    }

    //Move selection key handler
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
        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Bot.Moves.Count - 1);

        //Update screen
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Bot.Moves[currentMove]);

        //Select move -- Z for select || X for back
        if (Input.GetKeyDown(KeyCode.Z)) {
            var move = playerUnit.Bot.Moves[currentMove];
            if (move.PP == 0) { return; }
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        } else if (Input.GetKeyDown(KeyCode.X)) {
            if(playerUnit.Bot.HP <= 0) {
                partyScreen.SetMessageText("You have to choose a pokemon to continue");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            if(prevState == BattleState.AboutToUse) {
                prevState = null;
                StartCoroutine(SendNextTrainerBot());
            } else {
                ActionSelection();
            }
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
        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Bots.Count - 1);

        //Update screen
        partyScreen.UpdateMemberSelection(currentMember);

        //Select action(new Bot)
        if (Input.GetKeyDown(KeyCode.Z)) {
            var selectedMember = playerParty.Bots[currentMember];
            //Validity check
            if (selectedMember.HP <= 0) { partyScreen.SetMessageText("You can't send out a dead Bot"); return; }
            if (selectedMember == playerUnit.Bot) { partyScreen.SetMessageText("That Bot is already active"); return; }
            //Switch and update screen
            partyScreen.gameObject.SetActive(false);
            if (prevState == BattleState.ActionSelection) {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchBot));
            } else {
                state = BattleState.Busy;
                StartCoroutine(SwitchBot(selectedMember));
            }
        } else if (Input.GetKeyDown(KeyCode.X)) {
            //Go back
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    //Handle about to use
    void HandleAboutToUse() {
        //Screen update
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) {
            aboutToUseChoice = !aboutToUseChoice;
        }
        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        //Choice selection
        if (Input.GetKeyDown(KeyCode.Z)) {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true) {
                //yes
                prevState = BattleState.AboutToUse;
                OpenPartyScreen();
            } else {
                //no
                StartCoroutine(SendNextTrainerBot());
            }
        } else if (Input.GetKeyDown(KeyCode.X)) {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerBot());
        }
    }

    //Switch Bot
    IEnumerator SwitchBot(Bot newBot) {
        if (playerUnit.Bot.HP > 0) {
            yield return dialogBox.TypeDialog($"Return {playerUnit.Bot.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        playerUnit.Setup(newBot);
        dialogBox.SetMoveNames(newBot.Moves);
        yield return dialogBox.TypeDialog($"{newBot.Base.Name}!");
        if (prevState == null) {
            state = BattleState.RunningTurn;
        } else if (prevState == BattleState.AboutToUse) {
            prevState = null;
            StartCoroutine(SendNextTrainerBot());
        }
    }
    //Switch trainer bots
    IEnumerator SendNextTrainerBot() {
        state = BattleState.Busy;
        var nextBot = trainerParty.GetHealthyBot();
        enemyUnit.Setup(nextBot);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextBot.Base.Name}!");
        state = BattleState.RunningTurn;
    }
    
    //Handle escaping battle
    IEnumerator TryToEscape() {
        state = BattleState.Busy;
        if (isTrainerBattle) {
            yield return dialogBox.TypeDialog($"You can't run from trainer battle");
            state = BattleState.RunningTurn;
            yield break;
        }
        escapeAttempts++;
        int playerSpeed = playerUnit.Bot.Speed;
        int enemySpeed = enemyUnit.Bot.Speed;

        if(enemySpeed < playerSpeed) {
            yield return dialogBox.TypeDialog($"Ran away safetly");
            BattleOver(true);
        } else {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f) {
                yield return dialogBox.TypeDialog($"Ran away safetly");
                BattleOver(true);
            } else {
                yield return dialogBox.TypeDialog($"Caught trying to escape");
                state = BattleState.RunningTurn;
            }
        }
    }
}