using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollCript : MonoBehaviour
{
    private Rigidbody rBody;
    private Vector3 position, startPosition;

    [SerializeField] private float maxRandForceVal = 100f;
    [SerializeField] private float startRollingForce = 1600f;

    public string diceFaceNum;
    public bool isLanded = false;
    public bool firstThrow = false;

    public SoundEffectsScript soundEffects;   // assign in inspector (same as CircusBoardManager.soundEffects)

    private void Awake()
    {
        startPosition = transform.position;
        Initialize();
    }

    private void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        position = transform.position;
        transform.rotation = Random.rotation;
    }

    private void RollDiceInternal()
    {
        if (rBody == null)
            return;

        rBody.isKinematic = false;
        float forceX = Random.Range(0, maxRandForceVal);
        float forceY = Random.Range(0, maxRandForceVal);
        float forceZ = Random.Range(0, maxRandForceVal);

        // play dice‑throw sound just as we launch it into the air
        if (soundEffects != null)
        {
            soundEffects.PlayDiceHit();
        }

        rBody.AddForce(Vector3.up * Random.Range(800, startRollingForce));
        rBody.AddTorque(forceX, forceY, forceZ);
    }

    public void ResetDice()
    {
        transform.position = startPosition;
        firstThrow = false;
        isLanded = false;
        Initialize();
    }

    /// <summary>
    /// Public method to trigger a roll (used by AI or UI button).
    /// </summary>
    public void RollNow()
    {
        firstThrow = true;
        isLanded = false;
        RollDiceInternal();
    }

    private void Update()
    {
        if (rBody == null)
            return;

        // Mouse-controlled rolling
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    // Only allow new roll if dice has landed or this is first throw
                    if (!firstThrow || isLanded)
                    {
                        if (!firstThrow)
                            firstThrow = true;

                        isLanded = false;
                        RollDiceInternal();
                    }
                }
            }
        }
    }

    // still used by SideDetectScript to tell the logic that dice stopped,
    // but we no longer play the impact sound here
    public void OnDiceLanded()
    {
        isLanded = true;
    }
}
