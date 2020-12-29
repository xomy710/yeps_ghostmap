using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Cam_Rotate : MonoBehaviour
{

    public float move = 10f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void left()
    {


        this.transform.Rotate(0, -move, 0);


    }

    public void right()
    {
        this.transform.Rotate(0, move, 0);

    }

    public void up()
    {
        this.transform.Rotate(-move, 0, 0);

    }

    public void down()
    {
        this.transform.Rotate(move, 0, 0);

    }

    public void Scene_opening()
    {
        SceneManager.LoadScene("MAP");
    }
}
