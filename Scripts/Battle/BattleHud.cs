using DG.Tweening;
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
    [SerializeField] GameObject expBar;

    //Colors for Status
    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    //Store Variable
    Bot _bot;
    Dictionary<ConditionID, Color> statusColors;

    //Set data
    public void SetData(Bot bot) {
        _bot = bot;
        nameText.text = _bot.Base.Name;
        levelText.text = "LvL" + _bot.Level;
        hpBar.SetHP((float) _bot.HP / _bot.MaxHp);
        SetExp();

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
        _bot.OnStatusChanged += SetStatusText;
    }

    //Update status text on the hud
    void SetStatusText() {
        if(_bot.Status == null) {
            statusText.text = "";
        } else {
            statusText.text = _bot.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_bot.Status.Id];
        }
    }

    //Set Exp so the bar can scale
    public void SetExp() {
        if (expBar == null) { return; }
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }
    
    //Smooth Exp 
    public IEnumerator SetExpSmooth() {
        if (expBar == null) { yield break; }
        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    //Sets exp values between 0-1
    float GetNormalizedExp() {
        int currentLevelExp = _bot.Base.GetExpForLevel(_bot.Level);
        int nexttLevelExp = _bot.Base.GetExpForLevel(_bot.Level + 1);
        float normalizedExp = (float)(_bot.Exp - currentLevelExp) / (nexttLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    //Update HP
    public IEnumerator UpdateHP() {
        if (_bot.HpChanged) {
            yield return hpBar.SetHpSmooth((float)_bot.HP / _bot.MaxHp);
            _bot.HpChanged = false;
        }
    }
}
