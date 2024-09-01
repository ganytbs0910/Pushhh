using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using TMPro;
using EnhancedScrollerDemos.SnappingDemo;

public class UIController : MonoBehaviour
{
    SnappingDemo snappingDemo;

    void Start()
    {
        snappingDemo = GetComponent<SnappingDemo>();
        if (snappingDemo != null)
        {
            if (snappingDemo.isAutomaticMode == true)
            {
                Debug.Log("Automatic Mode is enabled");
            }
            else
            {
                Debug.Log("Automatic Mode is disabled");
            }
        }
    }
}