using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class UIManager : NetworkBehaviour
{
    public GameObject playButton;

    public TextMeshProUGUI[] playerNames;

    private Dictionary<ulong, int> clientToIndex = new();

    private int nextIndex = 0;

    private void Start()
    {
        foreach (var name in playerNames)
        {
            name.text = "";
        }
    }

    public void OnReadyButtonClick()
    {
        LobbyManager.Instance.ToggleReadyStatus();
    }
    public void OnPlayButtonClick()
    {
        LobbyManager.Instance.TransitionToGameScene();
    }

    public void SetPlayerName(ulong clientId, string newName)
    {
        if (!clientToIndex.ContainsKey(clientId))
        {
            int reuseIndex = -1;
            for (int i = 0; i < playerNames.Length; i++)
            {
                if (playerNames[i].text == "")
                {
                    reuseIndex = i;
                    break;
                }
            }

            if (reuseIndex != -1)
            {
                clientToIndex[clientId] = reuseIndex;
            }
            else
            {
                clientToIndex[clientId] = nextIndex;
                nextIndex++;
            }
        }

        int index = clientToIndex[clientId];
        if (index < playerNames.Length)
        {
            playerNames[index].text = "- " + newName;
        }
    }
    public void RemovePlayerName(ulong clientId)
    {
        if (clientToIndex.ContainsKey(clientId))
        {
            int index = clientToIndex[clientId];
            if (index < playerNames.Length)
            {
                playerNames[index].text = "";
            }

            clientToIndex.Remove(clientId);
        }
    }

    public void CheckHostButton()
    {
        if (IsHost)
        {
            playButton.SetActive(true);
        }
    }
}
