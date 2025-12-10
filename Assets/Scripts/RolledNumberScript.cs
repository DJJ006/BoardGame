using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class RolledNumberScript : MonoBehaviour
{
    DiceRollCript diceRollScript;
    [SerializeField]
    Text rolledNumberText;
    void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollCript>();
    }


    void Update()
    {
        if (diceRollScript != null)
        {
            if (diceRollScript.isLanded)
                rolledNumberText.text = diceRollScript.diceFaceNum;
            else
                rolledNumberText.text = "?";
        }else{
            Debug.LogWarning("DiceRollScript not found!");
        }
    }
}
