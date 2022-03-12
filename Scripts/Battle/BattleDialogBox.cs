using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//Controls the dialog box during battle
public class BattleDialogBox : MonoBehaviour {
    //Get data from various variables
    [SerializeField] Text dialogText;
    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text yesText;
    [SerializeField] Text noText;
    [SerializeField] int lettersPerSecond;
    [SerializeField] Color highlightedColor;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    //Get available actions and moves 
    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    //Set dialog to the battle dialog
    public void SetDialog(string dialog) {
        dialogText.text = dialog;
    }

    //Letter animation
    public IEnumerator TypeDialog(string dialog) {
        dialogText.text = "";
        foreach(var letter in dialog.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }
        yield return new WaitForSeconds(1f);
    }

    //Enable battle dialog box
    public void EnableDialogText(bool enabled) {
        dialogText.enabled = enabled;
    }

    //Enable action selector
    public void EnableActionSelector(bool enabled) {
        actionSelector.SetActive(enabled);
    }

    //Enable Move selector
    public void EnableMoveSelector(bool enabled) {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }    
    
    //Enable Choice selector
    public void EnableChoiceBox(bool enabled) {
        choiceBox.SetActive(enabled);
    }


    //Update and Highlight action selection
    public void UpdateActionSelection(int selectedAction) {
        for (int i=0; i<actionTexts.Count; ++i) {
            if (i == selectedAction) {
                actionTexts[i].color = highlightedColor;
            } else {
                actionTexts[i].color = Color.black;
            }
        }
    }    
    
    //Update and Highlight choiceBox selection
    public void UpdateChoiceBox(bool yesSelected) {
        if (yesSelected) {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        } else {
            yesText.color = Color.black;
            noText.color = highlightedColor;
        }
    }

    //Update and Highlight Move selection
    public void UpdateMoveSelection(int selectedMove, Move move) {
        for (int i=0; i<moveTexts.Count; ++i) {
            if(i == selectedMove) {
               moveTexts[i].color = highlightedColor;
            } else {
               moveTexts[i].color = Color.black;
            }
        }
        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
        if (move.PP == 0) {
            ppText.color = Color.red;
        } else {
            ppText.color = Color.black;
        }
    }

    //Get and set move names for current bot
    public void SetMoveNames(List<Move> moves) {
        for (int i=0; i<moveTexts.Count; ++i) {
            if (i < moves.Count) {
                moveTexts[i].text = moves[i].Base.Name;
            } else {
                moveTexts[i].text = "-";
            }
        }
    }
}
