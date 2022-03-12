using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Faceable directions
public enum FacingDirection { Up, Down, Left, Right }
public class CharacterAnimator : MonoBehaviour {
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] FacingDirection defaultDirection = FacingDirection.Down;

    //Paramaters
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    
    //Get faceable direction
    public FacingDirection DefaultDirection { get => defaultDirection; }

    //States
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator currentAnim;
    bool wasPreviouslyMoving;
    //Refrences
    SpriteRenderer spriteRenderer;

    private void Start() {
        //animation sets
        spriteRenderer = GetComponent<SpriteRenderer>();
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        currentAnim = walkDownAnim;
        SetFacingDirection(defaultDirection);
    }


    private void Update() {
        //Find aim
        var prevAnim = currentAnim;
        if (MoveX == 1) {
            currentAnim = walkRightAnim;
        } else if (MoveX == -1) {
            currentAnim = walkLeftAnim;
        } else if (MoveY == 1) {
            currentAnim = walkUpAnim;
        } else if (MoveY == -1) {
            currentAnim = walkDownAnim;
        }

        if(currentAnim != prevAnim || IsMoving != wasPreviouslyMoving) {
            currentAnim.Start();
        }
        //animation update
        if (IsMoving) {
            currentAnim.HandleUpdate();
        } else {
            spriteRenderer.sprite = currentAnim.Frames[0];
        } 
        wasPreviouslyMoving = IsMoving;
    }

    //Set faceing direction
    public void SetFacingDirection(FacingDirection dir) {
        switch (dir) {
            case FacingDirection.Left:
                MoveX = -1;
                break;
            case FacingDirection.Right:
                MoveX = 1;
                break;
            case FacingDirection.Up:
                MoveY = 1;
                break;
            default:
                MoveY = -1;
                break;
        }
    }
}