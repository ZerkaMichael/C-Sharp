using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//States
public enum GameState { FreeRoam, Battle, Dialog, Cutscene}

//Switch between battles and roam
public class GameController : MonoBehaviour {
    //Data
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    //State variable
    GameState state;

    //ON awake intialize conditionsDB
    public static GameController Instance { get; private set; }
    private void Awake() {
        Instance = this;
        ConditionsDB.Init();
    }

    //on start subscribe to events and handle
    private void Start() {
        playerController.OnEncountered += StartBattle;
        playerController.OnEnterTrainersView += (Collider2D trainerCollider) => {
            var trainer = trainerCollider.GetComponentInParent<TrainerController>();
            if(trainer != null) {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };
        battleSystem.OnBattleOver += EndBattle;
        DialogManager.Instance.OnShowDialog += () => { state = GameState.Dialog; };
        DialogManager.Instance.OnCloseDialog += () => { if (state == GameState.Dialog) { state = GameState.FreeRoam; } };
    }

    //Start battle function
    void StartBattle() {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        //Get bot party from playerController
        //Get wild bot from MapArea
        var playerParty = playerController.GetComponent<BotParty>();
        var wildBot = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildBot();
        battleSystem.StartBattle(playerParty, wildBot);
    }
    //Start battle with trainer function
    TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer) {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        //Get bot party from playerController
        this.trainer = trainer;
        var playerParty = playerController.GetComponent<BotParty>();
        var trainerParty = trainer.GetComponent<BotParty>();
        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }
    
    //End battle function
    void EndBattle(bool won) {
        if(trainer != null && won == true) {
            trainer.BattleLost();
            trainer = null;
        }
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    //Check and switch state
    private void Update() {
        switch (state) {
            case GameState.Battle:
                battleSystem.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.Instance.HandleUpdate();
                break;
            case GameState.Cutscene:
                break;
            default:
                playerController.HandleUpdate();
                break;
        }
    }
}
