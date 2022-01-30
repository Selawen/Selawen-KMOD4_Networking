using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Network.ServerBehaviour))]
public class ServerGameManager : MonoBehaviour
{
    private Network.ServerBehaviour server;

    private string[] noteColours;
    private KeyCode[] noteDirections;
    [SerializeField] [Range(1,4)] private int playercount;

    [SerializeField] private int bpm;
    private float beatTime;
    private float timer;

    // Start is called before the first frame update
    void Awake()
    {
        server = GetComponent<Network.ServerBehaviour>();
        playercount = server.playercount;
        beatTime = 60f / bpm;
        noteColours = new string[4]{"red", "blue", "green", "yellow" };
        noteDirections = new KeyCode[4]{KeyCode.A, KeyCode.W, KeyCode.S, KeyCode.D };
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            int pos = (int)Random.Range(0, 4-0.1f);
            int hitPlayer = (int)Random.Range(0, playercount + .9f);

            server.NoteSpawn(pos, hitPlayer);

            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = (int)Random.Range(1, 10);
        timer *= beatTime;
        //Debug.Log(timer);
    }
}
