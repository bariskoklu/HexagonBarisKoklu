using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public IntType score;
    public BoolType isBombActive;
    public TileClassListType allTiles;
    public IntType bombActionCount;

    public int numberOfActionsBeforeBombExplodes = 5;

    //Rastgele bir tileda bomba spawn etmeye yarar.
    public void SpawnBomb()
    {
        TileClass tileToSpawnBombOn = allTiles.tileList[UnityEngine.Random.Range(0, allTiles.tileList.Count - 1)];
        tileToSpawnBombOn.isItBombTile = true;
        isBombActive.value = true;
        bombActionCount.value = numberOfActionsBeforeBombExplodes;
    }
    //Bombanın patlama fonksiyonu. Oyunu restartlar.
    private void ExplodeBomb()
    {
        bombActionCount.value = numberOfActionsBeforeBombExplodes;
        isBombActive.value = false;
        GameManager.instance.RestartGame();
    }
    //Bombanın patlaması için kalan move sayısını azaltır.
    private void DecreaseBombActionCount()
    {
        bombActionCount.value -= 1;
        if (bombActionCount.value <= 0)
        {
            ExplodeBomb();
        }
    }
    //Aktif bir bomba varsa bombanın patlaması için kalan move sayısını azaltan fonksiyonu çağırır.
    public void CheckForBomb()
    {
        if (isBombActive.value)
        {
            DecreaseBombActionCount();
        }
    }
}
