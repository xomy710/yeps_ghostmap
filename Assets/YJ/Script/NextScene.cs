using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{

    public string S1;
    public string S2;
    public string S3;
    public string S4;
    public string S5;
    public string S6;
    public string S7;
    public string S8; 
    public string S9;
    public string S10;
    public string S11;
    public string S12;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Scene_1()
    {
        SceneManager.LoadScene(S1);
    }

    public void Scene_2()
    {
        SceneManager.LoadScene(S2);
    }


    public void Scene_3()
    {
        SceneManager.LoadScene(S3);
    }

    public void Scene_4()
    {
        SceneManager.LoadScene(S4);
    }

    public void Scene_5()
    {
        SceneManager.LoadScene(S5);
    }

    public void Scene_6()
    {
        SceneManager.LoadScene(S6);
    }

    public void Scene_7()
    {
        SceneManager.LoadScene(S7);
    }

    public void Scene_9()
    {
        SceneManager.LoadScene("1_9");
    }

    public void Scene_10()
    {
        SceneManager.LoadScene("1_10");
    }

    public void Scene_8()
    {
        SceneManager.LoadScene("1_8");
    }

    public void Scene_opening()
    {
        SceneManager.LoadScene("MAP_2021");
    }
}
