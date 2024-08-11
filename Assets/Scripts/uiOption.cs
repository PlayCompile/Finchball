using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class uiOption : MonoBehaviour
{
    public GameManager gameManager;
    public int selected = 0;
    public Color32 colorOn;
    public Color32 colorOff;
    public int fontSizeOn;
    public int fontSizeOff;
    public Text txtPrompt;
    public List<Text> txtOptions = new List<Text>();
    public InputActionAsset inputAss;
    private InputAction inputSelectLeft;
    private InputAction inputSelectRight;
    private InputAction inputSelectConfirm;

    private void OnEnable()
    {
        inputSelectLeft = inputAss.FindAction("selectLeft");
        inputSelectRight = inputAss.FindAction("selectRight");
        inputSelectConfirm = inputAss.FindAction("selectConfirm");
        updateSelection();
    }

    void Update()
    {
        if (inputSelectLeft.WasPressedThisFrame()) { selected--; updateSelection(); }
        if (inputSelectRight.WasPressedThisFrame()) { selected++; updateSelection(); }
        if (inputSelectConfirm.WasPressedThisFrame()) { confirmSelection(); }
    }

    void updateSelection()
    {
        // Wrap options when out of range
        if (selected < 0) { selected = txtOptions.Count - 1;  }
        if (selected > txtOptions.Count - 1) { selected = 0; }

        foreach (Text t in txtOptions)
        {
            t.color = colorOff;
            t.fontSize = fontSizeOff;
        }
        Text tsel = txtOptions[selected];
        tsel.color = colorOn;
        tsel.fontSize = fontSizeOn;

        if (gameManager.gameLevel == gameManager.numOfLevels)
        {
            txtPrompt.text = "PLAY AGAIN?";
        }
    }

    void confirmSelection()
    {
        // Play another game
        if (selected == 0)
        {
            if (gameManager.gameLevel == gameManager.numOfLevels)
            {
                SceneManager.LoadScene("Finchball");
            }
            else
            {
                gameManager.nextLevel = true;
            }
        }

        // Quit game
        if (selected == 1)
        {
            Application.Quit();
        }
    }
}
