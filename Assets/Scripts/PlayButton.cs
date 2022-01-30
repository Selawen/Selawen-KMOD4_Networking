using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private Network.ClientBehaviour client;
    public void StartGame()
    {
        if (client.connected)
        {
            client.StartGame();
        } else
        {
            GameObject.Find("MessageTxt").GetComponent<TextMeshProUGUI>().text = "Not connected to server yet";
        }
    }
}
