/**
 *  Create a simple choice dialog that works in the headset and using the gamepad for selection
 *  The gamepad arrows move you up and down the list and the x button selects. There are keyboard
 *  options for the editor.
 * 
 *  Version History
 *  V1.0 - July 27, 2020. Initial version
 * 
 *  Michael Jenkin
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog : MonoBehaviour
{

    public GameObject title;
    public GameObject choice;
    public GameObject instructions;

    private string[] choices;

    private int currentChoice = 0;
    private bool hasResponse = false;

    public void SetDialogTitle(string title)
    {
        this.title.GetComponent<TextMesh>().text = title;
        this.hasResponse = false;
    }

    public void SetDialogInstructions(string instructions)
    {
        this.instructions.GetComponent<TextMesh>().text = instructions;
        this.hasResponse = false;
    }

    public  void SetDialogChoices(string[] choices)
    {
        this.choices = choices;
        this.choice.GetComponent<TextMesh>().text = choices[0];
        this.currentChoice = 0;
        this.hasResponse = false;
    }

    public void SetDialogElements(string title, string[] choices)
    {
        this.choices = choices;

        this.title.GetComponent<TextMesh>().text = title;
        this.choice.GetComponent<TextMesh>().text = choices[0];
        this.currentChoice = 0;
        this.hasResponse = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        string[] q = { "hello", "world" };
        SetDialogElements("my new title", q);
        this.hasResponse = false;
    }

    public int GetResponse()
    {
        if (this.hasResponse)
            return this.currentChoice;
        return -1;
    }

    // Update is called once per frame
    void Update()
    {

     
        if (this.hasResponse)
            return;


        if (Input.GetKeyDown("up")|| Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            this.currentChoice = (this.currentChoice + this.choices.Length - 1) % this.choices.Length;
            this.choice.GetComponent<TextMesh>().text = choices[this.currentChoice];
        }
        if(Input.GetKeyDown("down")|| Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            this.currentChoice = (this.currentChoice + 1) % this.choices.Length;
            this.choice.GetComponent<TextMesh>().text = choices[this.currentChoice];
        }
        if(Input.GetKeyDown("x")|| Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            this.hasResponse = true;
        }
    }
}
