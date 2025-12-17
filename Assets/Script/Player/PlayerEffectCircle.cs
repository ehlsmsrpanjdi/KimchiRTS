using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class PlayerEffectCircle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        BuildingBase building = other.GetComponent<BuildingBase>();
        if (building != null)
        {
            building.OnSpecialEffect();
            LogHelper.Log("플레이어범위 안");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BuildingBase building = other.GetComponent<BuildingBase>();
        if (building != null)
        {
            building.OffSpecialEffect();
            LogHelper.Log("플레이어범위 밖");
        }
    }
}
