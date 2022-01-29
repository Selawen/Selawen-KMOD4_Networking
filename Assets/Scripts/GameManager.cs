using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Note> notes;
    [SerializeField] private List<Note> notepool;
    private string[] noteColours;
    private KeyCode[] noteDirections;
    [SerializeField] [Range(1,4)] private int playercount;

    [SerializeField] private int bpm;
    private float beatTime;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        beatTime = 60f / bpm;
        notes = new List<Note>();
        notepool = new List<Note>();
        noteColours = new string[4]{"red", "blue", "green", "yellow" };
        noteDirections = new KeyCode[4]{KeyCode.A, KeyCode.W, KeyCode.S, KeyCode.D };
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if(notes.Count >0)
        {
            if (notes[0].DestroyNote()){
                notepool.Add(notes[0]);
                notes.RemoveAt(0);
                Debug.Log("note destroyed");
            }
        }

        foreach (Note n in notes){
            n.MoveNotes(beatTime);
        }

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            int pos = (int)Random.Range(0, 4-0.1f);
            
            if (notepool.Count <1)
            {
            GameObject note = Instantiate(Resources.Load<GameObject>("Prefabs/Note"), new Vector3(pos*4, 5.5f, 0), transform.rotation);            
            notes.Add(new Note(note, noteColours[(int)Random.Range(0, playercount-0.1f)], noteDirections[pos]));
            } else {
                //GameObject note = Instantiate(Resources.Load<GameObject>("Prefabs/Note"), new Vector3(pos*4, 2, 0), transform.rotation);
                notepool[0].ResetNote(noteColours[(int)Random.Range(0, playercount-0.1f)], noteDirections[pos], new Vector3(pos*4, 5.5f, 0));    
                notes.Add(notepool[0]);
                notepool.RemoveAt(0);
            }
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
