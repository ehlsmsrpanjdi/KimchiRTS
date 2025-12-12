using System.Collections.Generic;
using UnityEngine;

public class BuildingElementUI : MonoBehaviour
{
    List<GameObject> cardList = new List<GameObject>();

    public void ResetElement()
    {
        foreach (GameObject card in cardList)
        {
            PoolManager.Instance.Push(card);
        }
        cardList.Clear();
    }

    public void AddCard(int _index)
    {
        GameObject cardObj = PoolManager.Instance.Pop(ResourceString.BuildCardName);
        cardObj.transform.SetParent(transform);
        cardObj.GetComponent<BuildCard>().InitCard(_index);
        cardList.Add(cardObj);
    }
}
