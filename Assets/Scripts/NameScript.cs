using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameScript : MonoBehaviour
{
    public Color nameColor = Color.white;

    private TMP_Text tmp;

    private void Awake()
    {
        Transform nameField = transform.Find("NameField");
        if (nameField != null)
        {
            tmp = nameField.GetComponent<TMP_Text>();
        }

        if (tmp == null)
        {
            tmp = GetComponent<TMP_Text>();
        }

        if (tmp == null)
        {
            Debug.LogError("NameScript: No TMP_Text found on or under this object. Please add a TextMeshPro component.");
        }
    }

    public void SetName(string name)
    {
        if (tmp == null) return;

        tmp.text = name;

        // Example: fixed yellow color
        //tmp.color = new Color32(255, 230, 0, 255);

        // OR use Color:
        tmp.color = nameColor;
    }
}
