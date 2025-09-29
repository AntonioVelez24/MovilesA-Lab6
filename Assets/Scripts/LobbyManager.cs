using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    public GameObject[] playerPrefabs;
    public Transform[] spawnPoints;
    public UIManager uiManager;

    private Dictionary<ulong, string> playerNames = new();

    public bool allPlayersReady = false;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        int playerIndex = NetworkManager.Singleton.ConnectedClientsList.Count - 1;

        if (playerIndex < spawnPoints.Length)
        {
            GameObject selectedPrefab = playerPrefabs[playerIndex];
            Transform spawnPoint = spawnPoints[playerIndex];

            GameObject player = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkObject netObj = player.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            string name = "Jugador " + clientId;
            playerNames[clientId] = name;

            UpdatePlayerNameClientRpc(clientId, name);

            Debug.Log("ConnectedPlayersCount " + NetworkManager.Singleton.ConnectedClientsList.Count);

            uiManager.CheckHostButton();
        }
        CheckAllPlayersReady();
    }
    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("Jugador desconectado: " + clientId);

        if (playerNames.ContainsKey(clientId))
        {
            playerNames.Remove(clientId);
        }

        Debug.Log("ConnectedPlayersCount " + NetworkManager.Singleton.ConnectedClientsList.Count);

        RemovePlayerNameClientRpc(clientId);

        CheckAllPlayersReady();
    }


    [ClientRpc]
    private void UpdatePlayerNameClientRpc(ulong clientId, string playerName)
    {
        uiManager.SetPlayerName(clientId, playerName);
    }

    [ClientRpc]
    private void RemovePlayerNameClientRpc(ulong clientId)
    {
        uiManager.RemovePlayerName(clientId);
    }

    public void ToggleReadyStatus()
    {
        OnReadyButtonClicked();
    }

    public void OnReadyButtonClicked()
    {
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObj != null)
        {
            var playerLobby = playerObj.GetComponent<PlayerLobby>();
            if (playerLobby != null)
            {
                playerLobby.ToggleReady();
            }
        }
    }

    public void CheckAllPlayersReady()
    {
        if (!IsHost) return;

        int readyCount = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj != null)
            {
                var playerLobby = playerObj.GetComponent<PlayerLobby>();
                if (playerLobby != null && playerLobby.IsReady())
                {
                    Debug.Log("Jugador " + client.ClientId + " está listo.");
                    readyCount++;
                    Debug.Log("Jugadores listos: " +  readyCount + "Count de players conectados" + NetworkManager.Singleton.ConnectedClientsList.Count);
                }
            }
        }

        if (readyCount == NetworkManager.Singleton.ConnectedClientsList.Count)
        {
            allPlayersReady = true;
        }
        else
        {
            allPlayersReady = false;
        }
    }

    public void TransitionToGameScene()
    {
        if (IsHost)
        {
            if (allPlayersReady)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("Game",LoadSceneMode.Single);
            }
            else
            {
                Debug.Log("No todos los jugadores están listos.");
            }
        }
    }
}
