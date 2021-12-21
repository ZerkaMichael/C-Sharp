using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
    //Hud for battle
public class BattleHud : MonoBehaviour {
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    //Colors for Status
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    //Store Variable
    Pokemon _pokemon;
    Dictionary<ConditionID, Color> statusColors;

    //Set data
    public void SetData(Pokemon pokemon) {
        _pokemon = pokemon;
        nameText.text = _pokemon.Base.Name;
        levelText.text = "LvL" + _pokemon.Level;
        hpBar.SetHP((float) _pokemon.HP / _pokemon.MaxHp);

        //Initalize status colors
        statusColors = new Dictionary<ConditionID, Color>() {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor},
        };
        //Set status text on hud
        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    //Update status text on the hud
    void SetStatusText() {
        if(_pokemon.Status == null) {
            statusText.text = "";
        } else {
            statusText.text = _pokemon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.Id];
        }
    }

    //Update HP
    public IEnumerator UpdateHP() {
        if (_pokemon.HpChanged) {
            yield return hpBar.SetHpSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HpChanged = false;
        }
    }
}
