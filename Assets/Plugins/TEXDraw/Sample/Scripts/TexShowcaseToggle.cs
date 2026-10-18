using UnityEngine;

public class TexShowcaseToggle : MonoBehaviour {

    GameObject[] objects;
    int current = 0;

	// Use this for initialization
	void Start () {
        objects = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            objects[i] = transform.GetChild(i).gameObject;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.anyKeyDown)
        {
            objects[current].SetActive(false);
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                current = (current == 0 ? objects.Length : current) - 1;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Return))
                current = (++current == objects.Length ? 0 : current);
            objects[current].SetActive(true);

        }

    }
}
