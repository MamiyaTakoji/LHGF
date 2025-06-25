using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScence : MonoBehaviour
{
    public int ScenceId;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener
        (delegate{
            UnityEngine.SceneManagement.SceneManager.LoadScene(ScenceId);
        });
    }

    // Update is called once per frame

}
