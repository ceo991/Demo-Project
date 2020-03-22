using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;//Namespace for accessing DoTween animation methods

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField] private int score;
    private int turn;
    [SerializeField] private int bombCount;
    [SerializeField] private int blownHexCount;

    [SerializeField] private Text scoreText;
    [SerializeField] private Text turnText;

    [SerializeField] private DOTweenAnimation GameOverPanelAnim;

    private Grid grid;

    private void Awake()
    {
        instance = this;
        bombCount = 0;
        blownHexCount = 0;

    }

    private void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(1))
        {
            turnText.text = "Turn: " + turn.ToString();
            scoreText.text = "Score: " + score.ToString();
        }
        grid = Grid.instance;
    }

    //Method for updating turn number
    public void UpdateTurn()
    {
        ++turn;
        turnText.text = "Turn: " + turn.ToString();
    }

    //Method for calculating and updating the score
    public void UpdateScore(int i)
    {
        blownHexCount += i;
        score = (5 * blownHexCount);
        scoreText.text = "Score: " + score.ToString();
        
        //Create a bomb every 1000 score
        if (score > 1000 * bombCount + 1000)
        {
            ++bombCount;
            grid.SetBombProduction();
        }
    }
    
    //Slide in game over panel
    public void GameOver()
    {
        GameOverPanelAnim.DOPlay();
    }

    //Loads Main Menu scene
    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    //Loads GamePlay scene
    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}