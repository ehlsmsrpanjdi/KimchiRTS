using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI resText;
    private void Reset()
    {
        resText = GetComponentInChildren<TextMeshProUGUI>();
    }


    public void UpdateResourceUI(int newAmount)
    {
        resText.text = newAmount.ToString();
    }
}
