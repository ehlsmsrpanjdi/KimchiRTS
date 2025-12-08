using UnityEngine;
using UnityEngine.UI;

public class DebugCreate : MonoBehaviour
{
    [SerializeField] Button btn;

    private void Reset()
    {
        btn = GetComponent<Button>();
    }

    private void Awake()
    {
        btn.onClick.AddListener(ClickBtn);
    }

    void ClickBtn()
    {
        DebugManager.Instance.CreateGhost();
    }
}
