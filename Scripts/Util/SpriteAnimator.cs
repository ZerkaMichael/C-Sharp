using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handle animating characters
public class SpriteAnimator {
    //Variables
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    int currentFrame;
    float frameRate;
    float timer;

    //Initalize the variables with the passed values
    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate=0.16f) {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start() {
        currentFrame = 0;
        timer = 0f;
        spriteRenderer.sprite = frames[0];
    }

    public void HandleUpdate() {
        timer += Time.deltaTime;
        if(timer > frameRate) {
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }
    
    public List<Sprite> Frames {
        get { return frames; }
    }
}
