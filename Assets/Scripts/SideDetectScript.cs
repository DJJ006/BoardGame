using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideDetectScript : MonoBehaviour
{
    // Set this in the Inspector on each side: 1,2,3,4,5,6
    [Range(1, 6)]
    public int faceValue = 1;

    private DiceRollCript diceRollScript;
    private Rigidbody diceBody;

    // How slow the dice must be to count as "landed"
    [SerializeField] private float landedVelocityThreshold = 0.05f;

    private void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollCript>();
        if (diceRollScript != null)
        {
            diceBody = diceRollScript.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerStay(Collider sideCollider)
    {
        if (diceRollScript == null || diceBody == null)
            return;

        // Only care while the die is actually rolling
        if (!diceRollScript.firstThrow)
            return;

        // Check if dice is basically stopped
        if (diceBody.velocity.magnitude < landedVelocityThreshold &&
            diceBody.angularVelocity.magnitude < landedVelocityThreshold)
        {
            diceRollScript.isLanded = true;
            // Store the numeric value as text, but it is now guaranteed 1–6
            diceRollScript.diceFaceNum = faceValue.ToString();
        }
        else
        {
            diceRollScript.isLanded = false;
        }
    }
}
