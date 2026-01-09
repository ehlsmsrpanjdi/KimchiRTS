using System.Collections.Generic;
using UnityEngine;

public class BuildingElementUI : MonoBehaviour
{
    List<GameObject> cardList = new List<GameObject>();

    public void ResetElement()
    {
        foreach (GameObject card in cardList)
        {
            Destroy(card);
        }
        cardList.Clear();
    }

    public void AddCard(int _index)
    {
        GameObject cardObj = AssetManager.Instance.GetByName(ResourceString.BuildCardName);
        GameObject spawnedCardObj = Instantiate(cardObj);
        spawnedCardObj.transform.SetParent(transform);
        spawnedCardObj.GetComponent<BuildCard>().InitCard(_index);
        cardList.Add(spawnedCardObj);
    }
}
