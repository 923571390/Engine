using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadBreak : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(LoadGameScene);
        return;
    }
    public void LoadGameScene()
    {
        SceneManager.LoadSceneAsync("break");
    }
}
