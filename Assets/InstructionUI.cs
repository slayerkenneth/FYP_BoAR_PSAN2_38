using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class InstructionUI : MonoBehaviour
{
    [Header("Reference")] 
    [SerializeField] private Text instructionText;

    [Header("Text Setting")] 
    public bool EnableBlinkingEffect = true;
    public bool EnableText = true;
    private string defaultString = "ScanningInstruction: ";
    private int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EnableText)
        {
            if (EnableBlinkingEffect)
            {
                if (counter % 10 > 8)
                {
                    instructionText.enabled = false;
                }
                else
                {
                    instructionText.enabled = true;
                }

                counter += 1;
            }
            else
            {
                instructionText.enabled = true;
            }
        }
        else
        {
            instructionText.enabled = false;
        }
    }

    public void SetInstructionText(string text, bool IsBlink)
    {
        if (text == null) return;
        instructionText.text = text;
        SetBlinkingTextEffect(IsBlink);
    }

    public void SetBlinkingTextEffect(bool IsBlink)
    {
        EnableBlinkingEffect = IsBlink;
    }
}
