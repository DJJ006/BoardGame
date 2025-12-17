using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRollCript : MonoBehaviour
{
    private Rigidbody rBody;
    private Vector3 position, startPosition;

    // Still snappy, but more controlled
    [SerializeField] private float maxRandForceVal = 600f;    // spin
    [SerializeField] private float launchForce = 2000f;       // overall strength

    public string diceFaceNum;
    public bool isLanded = false;
    public bool firstThrow = false;

    public SoundEffectsScript soundEffects;

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

        rBody.drag = 0.05f;
        rBody.angularDrag = 0.02f;
    }

    private void RollDiceInternal()
    {
        if (rBody == null)
            return;

        rBody.isKinematic = false;

        // strong random spin
        float torqueX = Random.Range(-maxRandForceVal, maxRandForceVal);
        float torqueY = Random.Range(-maxRandForceVal, maxRandForceVal);
        float torqueZ = Random.Range(-maxRandForceVal, maxRandForceVal);

        // mostly upward, with only a *small* horizontal component
        float horizRange = 0.25f; // clamp sideways movement
        Vector3 dir = new Vector3(
            Random.Range(-horizRange, horizRange),  // small left/right
            Random.Range(0.6f, 1.0f),               // strong up
            Random.Range(-horizRange, horizRange)); // small forward/back
        dir.Normalize();

        if (soundEffects != null)
        {
            soundEffects.PlayDiceHit();
        }

        rBody.AddForce(dir * launchForce, ForceMode.Impulse);
        rBody.AddTorque(new Vector3(torqueX, torqueY, torqueZ), ForceMode.Impulse);
    }

    public void ResetDice()
    {
        transform.position = startPosition;
        firstThrow = false;
        isLanded = false;
        Initialize();
    }

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

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.collider != null && hit.collider.gameObject == this.gameObject)
            {
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

    public void OnDiceLanded()
    {
        isLanded = true;
    }
}
