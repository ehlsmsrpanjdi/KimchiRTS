using UnityEngine;

public class ResBuilding : BuildingBase
{
    float currentTime = 0;
    int resAmount = 1;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }
    void Update()
    {
        if (BuildingOwnerId.Value != GameInstance.Instance.GetPlayerID())
        {
            return;
        }

        currentTime += Time.deltaTime;
        if (currentTime >= 1f)
        {
            currentTime = 0f;
            GameInstance.Instance.GetPlayer().playerResource.AddResource(resAmount);
        }
    }
}
