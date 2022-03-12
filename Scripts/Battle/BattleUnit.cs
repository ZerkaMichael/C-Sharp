using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


//Draw units on screen from sprite file - Based on player being bottom (back)
public class BattleUnit : MonoBehaviour{
    //Data
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    //Player check properity
    public bool IsPlayerUnit {
        get { return isPlayerUnit; }
    }

    //Hud properity
    public BattleHud Hud {
        get { return hud; }
    }

    //Get and set current Bot
    public Bot Bot { get; set; }

    //Animation Variables
    Image image;
    Vector3 originalPos;
    Color originalColor;

    //Get component image function
    private void Awake() {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition; //Local position for canvas not for world position
        originalColor = image.color;
    }

    //Determine isPlayer and set correct sprite
    public void Setup(Bot bot) {
        Bot = bot;
        if (isPlayerUnit) {
            image.sprite = Bot.Base.BackSprite;
        }else{
            image.sprite = Bot.Base.FrontSprite;
        }
        hud.gameObject.SetActive(true);
        hud.SetData(bot);
        image.color = originalColor;
        PlayerEnterAnimation();
    }

    //Turn hud off
    public void Clear() {
        hud.gameObject.SetActive(false);
    }

    //Play enter antimations for both pokemon
    public void PlayerEnterAnimation() {
        if (isPlayerUnit) {
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        } else {
            image.transform.localPosition = new Vector3(500f, originalPos.y);
        }
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    //Play attack animations for both pokemon -- Move pokemon on attack and move back
    public void PlayAttackAnimation() {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit) {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        } else {
            sequence.Append(image.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));
        }
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    //Play hit anmation for both pokemon -- Change color 
    public void PlayHitAnimation() {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }
    //Play faint animation for both pokemon -- Move pokemon and fade to 0 alpha
    public void PlayFaintAnimation() {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }
}
