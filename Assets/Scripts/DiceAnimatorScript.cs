using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceAnimatorScript : MonoBehaviour
{

    Animator animator;
    
    void Start()
    {
       animator = GetComponent<Animator>();
    }

    public void RollDice()
    {
               animator.SetBool("IsRooling", true);
    }

    public void StopDice()
    {
        animator.SetBool("IsRooling", false);
    }
}
