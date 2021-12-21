using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//States
public enum GameState { FreeRoam, Battle}

//Switch between battles and roam
public class GameController : MonoBehaviour {
    //Data
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;

    //State variable
    GameState state;
 
    //ON awake intialize conditionsDB
    private void Awake() {
        ConditionsDB.Init();
    }

    //on start subscribe to events and handle
    private void Start() {
        playerController.OnEncountered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;
    }

    //Start battle function
    void StartBattle() {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        //Get pokemon party from playerController
        //Get wild pokemon from MapArea
        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();
        battleSystem.StartBattle(playerParty, wildPokemon);
    }
    
    //End battle function
    void EndBattle(bool won) {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    //Check and switch state
    private void Update() {
        if (state == GameState.FreeRoam) {
            playerController.HandleUpdate();
        } else if (state == GameState.Battle) {
            battleSystem.HandleUpdate();
        }
    }
}
