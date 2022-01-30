using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoinRemoteButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ipText;
    [SerializeField] string ip;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Join()
    {
        ip = ipText.text;
        if(!(ip==""))
            FindObjectOfType<Network.ClientBehaviour>().JoinServer(ip);
        else
            FindObjectOfType<Network.ClientBehaviour>().JoinServer();
    }
}
