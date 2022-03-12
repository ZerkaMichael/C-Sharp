using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour { 
    //Hud for party
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color highlightColor;

    //Store Variable
    Bot _bot;


    //Set data
    public void SetData(Bot bot) {
        _bot = bot;
        nameText.text = bot.Base.Name;
        levelText.text = "LvL" + bot.Level;
        hpBar.SetHP((float)bot.HP / bot.MaxHp);
    }

    //Highlight member in party screen
    public void SetSelected(bool selected) {
        if (selected) {
            nameText.color = highlightColor;
        } else {
            nameText.color = Color.black;
        }
    }
}
