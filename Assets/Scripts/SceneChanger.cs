using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public void closeGame()
    {
        StartCoroutine(Quit());
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(0.8f);
        Application.Quit();
    }
}
