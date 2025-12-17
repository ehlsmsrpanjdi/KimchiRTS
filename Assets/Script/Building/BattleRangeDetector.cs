using System.Collections.Generic;
using UnityEngine;

public class BattleRangeDetector : MonoBehaviour
{
    List<EntityBase> monsterList = new List<EntityBase>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            EntityBase entity = other.GetComponent<EntityBase>();
            if (entity == null)
            {
                LogHelper.LogError("몬스터가 아닌데 충돌함");
            }
            monsterList.Add(entity);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            EntityBase entity = other.GetComponent<EntityBase>();
            if (entity == null)
            {
                LogHelper.LogError("몬스터가 아닌데 충돌함");
            }
            monsterList.Remove(entity);
        }
    }

    public EntityBase GetClosestEntity(Vector3 position)
    {
        // null이거나 파괴된 엔티티 제거
        monsterList.RemoveAll(e => e.isActiveAndEnabled == false);

        if (monsterList.Count == 0)
            return null;

        EntityBase closest = null;
        float minDistance = float.MaxValue;

        foreach (EntityBase entity in monsterList)
        {
            float distance = Vector3.Distance(position, entity.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = entity;
            }
        }

        return closest;
    }
}
