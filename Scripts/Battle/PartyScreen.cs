using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour {

    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;

    //Get all the pokemon in party
    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }
    //Set the data of each member of the party
    public void SetPartyData(List<Pokemon> pokemons) {
        this.pokemons = pokemons;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < pokemons.Count) {
                memberSlots[i].SetData(pokemons[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }

            messageText.text = "Choose a Pokemon";
        }
    }
    //Updates party selection screen
    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < pokemons.Count; i++) {
            if (i == selectedMember) {
                memberSlots[i].SetSelected(true);
            } else {
                memberSlots[i].SetSelected(false);
            }
        }
    }
    //Set message text
    public void SetMessageText(string message) {
        messageText.text = message;
    }
}
