using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable {
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    [SerializeField] Sprite sprite;
    [SerializeField] string name;

    //state
    bool battleLost = false;

    Character character;


    private void Awake() {
        character = GetComponent<Character>();
    }

    private void Start() {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update() {
        character.HandleUpdate();
    }

    //When player interacts with - Look towards and show dialog
    public void Interact(Transform initiator) {
        character.LookTowards(initiator.position);
        if (!battleLost) {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {}));
        } else {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle, () => {}));
        }
    }

    //Triggering trainerbattle 
    public IEnumerator TriggerTrainerBattle(PlayerController player) {
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Show Dialog
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
            GameController.Instance.StartTrainerBattle(this);
        }));
    }

    //Handle trainers after lost battle
    public void BattleLost() {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    //Find and set Fov angle
    public void SetFovRotation(FacingDirection dir) {
        float angle = 0f;
        switch (dir) {
            case FacingDirection.Up:
                angle = 180f;
                break;
            case FacingDirection.Left:
                angle = 270f;
                break;
            case FacingDirection.Right:
                angle = 90f;
                break;
            default:
                angle = 0f;
                break;
        }
        //Set rotation as vector
        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    //Properties
    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
