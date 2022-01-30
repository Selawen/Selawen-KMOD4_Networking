using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Note> notes;
    [SerializeField] private List<Note> notepool;
    private string[] noteColours;
    private string playercolour = "grey";
    [SerializeField] TMPro.TextMeshProUGUI playertext;

    private int score = 0;

    private enum NoteDirection
    {
        LEFT, UP, DOWN, RIGHT
    }
    static Dictionary<NoteDirection, KeyCode> directions = new Dictionary<NoteDirection, KeyCode> {
            { NoteDirection.LEFT, KeyCode.A },
            { NoteDirection.UP, KeyCode.W },
            { NoteDirection.DOWN, KeyCode.S},
            { NoteDirection.RIGHT, KeyCode.D},
        };

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

    }


    // Update is called once per frame
    void Update()
    {


        foreach (Note n in notes){
            n.MoveNotes(beatTime);
        }

        GetHits();        
        
        if(notes.Count >0)
        {
            if (notes[0].DestroyNote()){
                notepool.Add(notes[0]);
                notes.RemoveAt(0);
                Debug.Log("note destroyed");
            }
        }
    }

    public void SetPlayerColour(int colourIndex)
    {
        playercolour = noteColours[colourIndex];
        playertext.text = "Your colour: " + playercolour;
    }

    public void SpawnNote(int pos, int colour)
    {
        if (notepool.Count < 1)
        {
            GameObject note = Instantiate(Resources.Load<GameObject>("Prefabs/Note"), new Vector3(pos * 4, 5.5f, 0), transform.rotation);
            notes.Add(new Note(note, noteColours[colour], directions[(NoteDirection)pos]));
        }
        else
        {
            //GameObject note = Instantiate(Resources.Load<GameObject>("Prefabs/Note"), new Vector3(pos*4, 2, 0), transform.rotation);
            notepool[0].ResetNote(noteColours[colour], directions[(NoteDirection)pos], new Vector3(pos * 4, 5.5f, 0));
            notes.Add(notepool[0]);
            notepool.RemoveAt(0);
        }
    }

    private void GetHits()
    {
        if (notes.Count < 1)
        {
            return;
        }

        bool looping = true;
        int i = 0;
        while (looping)
        {
            looping = false;

            if (notes[i].note.GetComponent<MeshRenderer>().material.name == "grey")
            {
                looping = true;
                i++;
                continue;
            }

            if (Input.GetKeyDown(notes[i].key) && playercolour == notes[i].noteColour && notes[i].note.transform.position.y < -2.5f && notes[i].note.transform.position.y > -3.5f )
            {
                score += 100;
                notes[i].note.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/grey");
                notepool.Add(notes[i]);
                    
                looping = true;
                i++;
            }
        }
    }

    private void Hit(Note hitNote)
    {
        
    }
}
