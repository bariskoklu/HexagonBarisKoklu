using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton class. Oyunun resetlenmesi için kullanılıyor.

    public enum GameStates
    {
        InGameState,
        PausedState
    }

    public static GameManager instance = null;

    public TileClassListType allTiles;
    public TileClassListType selectedTiles;
    public IntType score;
    public BoolType isBombActive;
    public GameStates currentGameState;

    public GameObject UIObject;

    public bool isTilesRotating = false;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        ResetResources();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void RestartGame()
    {
        ResetResources();
        currentGameState = GameStates.InGameState;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetResources()
    {
        allTiles.tileList.Clear();
        selectedTiles.tileList.Clear();
        score.value = 0;
        isBombActive.value = false;
    }

    public void ChangeState(GameStates gameState)
    {
        currentGameState = gameState;
        UIObject.SetActive(currentGameState == GameStates.PausedState ? true : false);
    }
}
