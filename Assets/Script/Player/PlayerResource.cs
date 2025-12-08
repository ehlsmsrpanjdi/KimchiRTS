using System;
using Unity.Netcode;

public class PlayerResource : NetworkBehaviour
{
    public NetworkVariable<int> Resources = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ★ UI 업데이트용 이벤트 (여기서 직접 관리)
    public Action<int> OnChangeResource;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // NetworkVariable 변경 시 자동 호출
        Resources.OnValueChanged += OnResourceChanged;

        // 초기값 UI 업데이트
        if (IsOwner)
        {
            OnChangeResource?.Invoke(Resources.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        Resources.OnValueChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(int previousValue, int newValue)
    {
        // 내 플레이어만 UI 업데이트
        if (IsOwner)
        {
            OnChangeResource?.Invoke(newValue);
            LogHelper.Log($"자원: {previousValue} → {newValue}");
        }
    }

    // 자원 체크
    public bool HasEnoughResource(int amount)
    {
        return Resources.Value >= amount;
    }

    // 자원 사용
    public bool TrySpendResource(int amount)
    {
        if (!IsOwner) return false;

        // 로컬 체크
        if (!HasEnoughResource(amount))
        {
            LogHelper.LogWarrning("자원 부족!");
            return false;
        }

        // Server에 요청
        SpendResourceServerRpc(amount);
        return true;
    }

    [Rpc(SendTo.Server)]
    private void SpendResourceServerRpc(int amount)
    {
        // Server에서 검증
        if (Resources.Value >= amount)
        {
            Resources.Value -= amount;
            // NetworkVariable이 자동으로 동기화 → OnValueChanged 발생!
        }
        else
        {
            LogHelper.LogWarrning("치팅 시도 감지!");
        }
    }

    // 자원 추가
    public void AddResource(int amount)
    {
        if (!IsOwner) return;
        AddResourceServerRpc(amount);
    }

    [Rpc(SendTo.Server)]
    private void AddResourceServerRpc(int amount)
    {
        Resources.Value += amount;
        // NetworkVariable이 자동으로 동기化!
    }

    // 현재 자원량
    public int GetCurrentResource()
    {
        return Resources.Value;
    }
}