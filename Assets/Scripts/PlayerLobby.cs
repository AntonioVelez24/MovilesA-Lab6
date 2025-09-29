using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerLobby : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;

    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(false);

    private void Start()
    {
        UpdateStatusText(isReady.Value);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        isReady.OnValueChanged += OnReadyStateChanged;

        UpdateStatusText(isReady.Value);
    }

    public override void OnNetworkDespawn()
    {
        isReady.OnValueChanged -= OnReadyStateChanged;
    }

    [ServerRpc]
    public void SetReadyStatusServerRpc(bool ready)
    {
        Debug.Log("Cliente " + OwnerClientId + " cambió a estado: " + (ready ? "Ready" : "Not Ready"));
        isReady.Value = ready;
    }
    private void OnReadyStateChanged(bool oldValue, bool newValue)
    {
        UpdateStatusText(newValue);

        if (IsServer)
        {
            LobbyManager.Instance.CheckAllPlayersReady();
        }
    }

    private void UpdateStatusText(bool ready)
    {
        if (statusText != null)
        {
            statusText.text = ready ? "Ready" : "Not Ready";
        }
    }

    public void ToggleReady()
    {
        if (IsOwner)
        {
            bool newValue = !isReady.Value;
            SetReadyStatusServerRpc(newValue);
        }
    }

    public bool IsReady()
    {
        return isReady.Value;
    }
}
