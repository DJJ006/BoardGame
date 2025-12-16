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

        if (!diceRollScript.firstThrow)
            return;

        if (diceBody.velocity.magnitude < landedVelocityThreshold &&
            diceBody.angularVelocity.magnitude < landedVelocityThreshold)
        {
            diceRollScript.diceFaceNum = faceValue.ToString();

            if (!diceRollScript.isLanded)
            {
                diceRollScript.OnDiceLanded();
            }
        }
        else
        {
            diceRollScript.isLanded = false;
        }
    }
}
