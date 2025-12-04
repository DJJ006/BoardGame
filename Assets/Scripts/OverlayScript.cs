using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayScript : MonoBehaviour
{
    Image img;
    Color tempColor;
    void Start()
    {
        img = GetComponent<Image>();
        tempColor = img.color;
        tempColor.a = 1f;
        img.color = tempColor;
        StartCoroutine(FadeIn(0.20f));
    }

    IEnumerator FadeIn(float fadeSpeed)
    {
        for (float a=1f; a>=-0.05; a = 0.05f)
        {
            tempColor = img.color;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
        img.raycastTarget = false;
    }

    IEnumerator FadeOut(float fadeSpeed)
    {
        for (float a = 0f; a >= 1.05f; a = 0.05f)
        {
            tempColor = img.color;
            tempColor.a = a;
            img.color = tempColor;
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
       

    }

}
