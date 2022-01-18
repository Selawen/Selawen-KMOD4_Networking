using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private Color noteColour;
    [SerializeField] private GameObject notePrefab;
    private GameObject note;
    Vector3 down = new Vector3(0, -1, 0);

    public Note(Color colour)
    {
        noteColour = colour;
        note = Instantiate(notePrefab);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        note.transform.Translate(down);
        if (note.transform.position.y <= -5)
        {
            //ToDo: make objectpool
            Destroy(this);
        }
    }
}
