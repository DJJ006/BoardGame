using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideDetectScript : MonoBehaviour
{
    DiceRollCript diceRollScript;

    void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollCript>();
    }

    
    private void OnTriggerStay(Collider sideCollider)
    {
        if(diceRollScript != null)
        {
            if(diceRollScript.GetComponent<Rigidbody>().velocity == Vector3.zero)
            {
                diceRollScript.isLanded = true;
                diceRollScript.diceFaceNum = sideCollider.name;
            }
            else
            {
                diceRollScript.isLanded = false;
            }
        }
        
    }
}
