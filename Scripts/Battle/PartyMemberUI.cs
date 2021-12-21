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
    Pokemon _pokemon;


    //Set data
    public void SetData(Pokemon pokemon) {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "LvL" + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp);
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
