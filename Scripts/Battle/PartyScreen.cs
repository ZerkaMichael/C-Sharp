using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour {

    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Bot> bots;

    //Get all the bots in party
    public void Init() {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }
    //Set the data of each member of the party
    public void SetPartyData(List<Bot> bots) {
        this.bots = bots;
        for (int i = 0; i < memberSlots.Length; i++) {
            if (i < bots.Count) {
                memberSlots[i].SetData(bots[i]);
            } else {
                memberSlots[i].gameObject.SetActive(false);
            }

            messageText.text = "Choose a Bot";
        }
    }
    //Updates party selection screen
    public void UpdateMemberSelection(int selectedMember) {
        for (int i = 0; i < bots.Count; i++) {
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
