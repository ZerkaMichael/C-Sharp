using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour{
    //Variables
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask interactableLayer;
    public LayerMask grassLayer;
    private bool isMoving;
    private Vector2 input;
    private Animator animator;

    public event Action OnEncountered;

    //On awake set the animator
    private void Awake() {
        animator = GetComponent<Animator>();
    }

    //On update get and determine if movement is acceptable
    public void HandleUpdate() {
        if (!isMoving) {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if(input != Vector2.zero) {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if (isWalkable(targetPos)) {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
        //Get interaction input
        if (Input.GetKeyDown(KeyCode.Z)) {
            Interact();
        }
    }

    //Interaction
    void Interact() {
        //Find facing direction
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        //Determine the position that the player can interact with
        var interactPos = transform.position + facingDir;

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
        if (collider != null) {
            //collider.GetComponent<NPCController>();
        }
    }

    //Movement math
    IEnumerator Move(Vector3 targetPos) {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon) {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounters();
    }

    //Bool for determing if tile is walkable
    private bool isWalkable(Vector3 targetPos) {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer | interactableLayer) != null) {
            return false;
        }
        return true;
    }

    //Transform a layer to an encounter layer - 10% chance of encountering every action a wild fight
    private void CheckForEncounters() {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null) {
            if (UnityEngine.Random.Range(1, 101) <= 10) {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }
}

