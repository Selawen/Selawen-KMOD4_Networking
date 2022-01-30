using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] public string noteColour;
    public KeyCode key;
    //[SerializeField] private GameObject notePrefab;
    public GameObject note;
    Vector3 down = new Vector3(0, -0.01f, 0);

    public Note(GameObject _note, string _colour, KeyCode _key)
    {
        noteColour = _colour;
        note = _note;
        key = _key;
        //Debug.Log("Materials/" + _colour);
        //Debug.Log(Resources.Load<Material>("Materials/" + _colour));
        note.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/" + noteColour);
    }

    // Start is called before the first frame update
    void Start()
    {
        //notePrefab = Resources.Load<GameObject>("Prefabs/Note");
    }

    public void ResetNote(string _colour, KeyCode _key, Vector3 _position)
    {
        noteColour = _colour;
        key = _key;
        note.transform.position = _position;
        note.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/" + noteColour);
    }
    // Update is called once per frame
    public void MoveNotes(float bpmMultiplier)
    {
        note.transform.Translate(down/bpmMultiplier);
        
    }

    public bool DestroyNote(bool destroy) { 
        return destroy; 
    }

    public bool DestroyNote(){
    if (note.transform.position.y <= -7.5f)
        {
            return true;        
        } else {return false;}
    }

    public bool Hit(KeyCode _key, string _colour)
    {
        return (_key == key && _colour == noteColour);
    }
}
