using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;
    int currentLine = 0;
    Dialog dialog;
    bool isTyping;
    public event Action OnShowDialog;
    public event Action OnCloseDialog;
    Action onDialogFinished;
    public bool IsShowing { get; private set; }


    public static DialogManager Instance { get; private set; }
    private void Awake() {
        Instance = this;
    }

    //Shows the Dialog
    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null) {
        //Wait until end of frame to not conflict
        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke();
        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;
        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    //Update
    public void HandleUpdate() {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping) {
            ++currentLine;
            if(currentLine < dialog.Lines.Count) {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            } else {
                currentLine = 0;
                IsShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }


    //Letter animation
    public IEnumerator TypeDialog(string line) {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray()) {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
