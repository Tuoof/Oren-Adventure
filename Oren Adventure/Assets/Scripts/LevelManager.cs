using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject cantOpenLevelWindow;
    public GameObject catHead;
    [SerializeField] private Transform[] levelPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        catHeadPos();
    }
    public void catHeadPos()
    {
        catHead.transform.position = levelPos[PlayerPrefs.GetInt("LevelWinCheck")].position;
    }
    public void LevelCheck(int i, int j)
    {
        if(PlayerPrefs.GetInt("LevelWinCheck") >=i)
        {
            SceneManager.LoadScene(j);
        }
        else
        {
            cantOpenLevelWindow.SetActive(true);
        }
    }   
    public void closeCantPlayWindow()
    {
        cantOpenLevelWindow.SetActive(false);
    }
    public void PlayerPrefReset()
    {
        PlayerPrefs.SetInt("LevelWinCheck", 1);
        Debug.Log("PF Reseted");
    }
    public void level1_1()
    {
        Debug.Log("button pressed!");
        LevelCheck(1, 5);
    }
    public void level1_2()
    {
        Debug.Log("current playerpref: "+ PlayerPrefs.GetInt("LevelWinCheck"));
        LevelCheck(2, 6);
    }
}
