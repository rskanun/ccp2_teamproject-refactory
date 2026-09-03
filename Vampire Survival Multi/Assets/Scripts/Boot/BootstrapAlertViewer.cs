using System;
using UnityEngine;

public class BootstrapAlertViewer : MonoBehaviour
{
    public void ViewAlert(string content, string buttonText, Action handler = null)
    {
        Debug.Log($"[Alert] {content}");
    }
}