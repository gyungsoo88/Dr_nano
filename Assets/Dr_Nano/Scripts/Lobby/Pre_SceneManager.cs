using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pre_SceneManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(NextScene());
    }

    IEnumerator NextScene()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("Lobby");
    }

}
