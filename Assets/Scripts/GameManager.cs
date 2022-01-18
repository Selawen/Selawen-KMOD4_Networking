using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Note> notes;
    private Color[] noteColours;
    [SerializeField] private int playercount;

    [SerializeField] private int bpm;
    private float timer = 0;

    // Start is called before the first frame update
    void Start()
    {
        notes = new List<Note>();
        noteColours = new Color[4]{Color.red, Color.blue, Color.green, Color.yellow };
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            notes.Add(new Note(noteColours[(int)Random.Range(0, playercount+0.9f)]));
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = (int)Random.Range(1, 10);
        timer *= 60 / bpm;
    }
}
