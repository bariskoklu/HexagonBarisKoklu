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

    public void SpawnBomb()
    {
        TileClass tileToSpawnBombOn = allTiles.tileList[UnityEngine.Random.Range(0, allTiles.tileList.Count - 1)];
        tileToSpawnBombOn.isItBombTile = true;
        isBombActive.value = true;
        bombActionCount.value = numberOfActionsBeforeBombExplodes;
    }

    public void DestroyBomb()
    {
        //TODO: Oyunu bitir.
    }
}
