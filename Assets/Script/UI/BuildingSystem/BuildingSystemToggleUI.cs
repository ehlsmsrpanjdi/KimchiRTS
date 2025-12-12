using UnityEngine;
using UnityEngine.UI;

public class BuildingSystemToggleUI : MonoBehaviour
{
    public Image toggleImg;

    private void Reset()
    {
        toggleImg = GetComponent<Image>();
    }
}
