using UnityEngine;

public class BattleGameMode : MonoBehaviour
{
    public bool IsLoadEnd { get; private set; } = false;
    private void Awake()
    {
        IsLoadEnd = false;
        TempLoad();
    }

    public async void TempLoad()
    {
        IsLoadEnd = false;
        await LoadManager.Instance.LoadTemp();  // 이 줄이 핵심
        IsLoadEnd = true;
        LogHelper.Log("로드 완리요");
    }
}
