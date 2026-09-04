using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class Alert : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI confirmButtonText;

    private Action confirmCallback;

    public void Show(string content, string confirmText, Action onConfirm = null)
    {
        contentText.text = content;
        confirmButtonText.text = confirmText;
        confirmCallback = onConfirm;

        gameObject.SetActive(true);
    }

    public void OnClick()
    {
        gameObject.SetActive(false);

        confirmCallback?.Invoke();
    }
}