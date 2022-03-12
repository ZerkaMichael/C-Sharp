using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] Sprite sprite;
    [SerializeField] string name;
    //Variables
    private Vector2 input;
    private Character character;
    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainersView;

    //On awake set the animator
    private void Awake() {
        character = GetComponent<Character>();
    }

    //On update get and determine if movement is acceptable
    public void HandleUpdate() {
        if (!character.IsMoving) {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if (input != Vector2.zero) {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }
        character.HandleUpdate();
        //Get interaction input
        if (Input.GetKeyDown(KeyCode.Z)) {
            Interact();
        }
    }

    //Interaction
    void Interact() {
        //Find facing direction
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        //Determine the position that the player can interact with
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null) {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    //Do move over
    private void OnMoveOver() {
        CheckForEncounters();
        CheckIfInTrainersView();
    }

    //Check for encounter when on grasslayer
    private void CheckForEncounters() {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.GrassLayer) != null) {
            if (UnityEngine.Random.Range(1, 101) <= 10) {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }
    //Trainer fov check
    private void CheckIfInTrainersView() {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FovLayer);
        if (collider != null) {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }
    //Properties
    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}