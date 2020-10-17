using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.Mathematics;
using System;
using System.Runtime.CompilerServices;

public class RotateTiles : MonoBehaviour
{
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public TileClassListType allTiles;
    public TileClassListType selectedTiles;
    public IntType score;
    public BoolType isBombActive;

    public float totalRotationTime = 2.0f;
    public int scoreMultiplier = 5;

    [NonSerialized]public bool isRotatingCounterClockwise;

    private BombScript bombScript;
    private DrawAndSetTiles DrawAndSetTilesScript;
    private bool isRotationComplete = false;
    private float rotationTimer = 0.0f;
    private List<TileClass> tilesToBeDeleted = new List<TileClass>();
    private List<TileClass> tilesToCheck = new List<TileClass>();
    // Start is called before the first frame update
    void Start()
    {
        bombScript = gameObject.GetComponent<BombScript>();
        DrawAndSetTilesScript = gameObject.GetComponent<DrawAndSetTiles>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
        {
            isRotatingCounterClockwise = false;
            this.HandleTilesRotation(selectedTiles.tileList, isRotatingCounterClockwise, true);
        }

        if (Input.GetKeyDown(KeyCode.Y) && selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
        {
            isRotatingCounterClockwise = true;
            this.HandleTilesRotation(selectedTiles.tileList, isRotatingCounterClockwise, true);
        }


        this.HandleRotation();

    }

    private void HandleRotation()
    {
        //isTileRotating durumu oyuncu bir tile seçtikten sonra bu tilelı sağa veya sola döndürme durumu gerçekleşmekteyse true durumdadır.
        if (GameManager.instance.isTilesRotating)
        {
            if (rotationTimer > 0)
            {
                rotationTimer -= Time.deltaTime;
            }
            else
            {
                rotationTimer = 0.0f;
                isRotationComplete = true;
            }
        }
        //Rotasyon yeni bitmişse bu kod çalıştırılır.
        if (GameManager.instance.isTilesRotating && isRotationComplete)
        {
            //Rotasyon bittiği zaman, tilelar arasında aynı renk olan eşleşen tilelar var mı kontrolünün yapıldığı yer.
            tilesToBeDeleted = this.FindMatches();
            //tilesToCheck.AddRange(selectedTiles.tileList);
            //for (int i = 0; i < tilesToCheck.Count; i++)
            //{
            //    if (this.CheckForMatchesForOneTile(tilesToCheck[i]))
            //    {
            //        result = true;
            //    }
            //}

            bombScript.CheckForBomb();
            //Eğer eşleşen (yani yok edilecek) tile olduysa o tileları yoketme işlemi ve eşleyen hiçbir tile olmadıysa tileları eski halina döndürme işlemi burada yapılır.
            if (tilesToBeDeleted.Count == 0)
            {
                this.HandleTilesRotation(selectedTiles.tileList, !isRotatingCounterClockwise, true);
            }
            else
            {
                this.DeleteTiles(tilesToBeDeleted);
            }
            GameManager.instance.isTilesRotating = false;
            isRotationComplete = false;
        }
    }

    //Tek bir tile için bu tilela eşleşen tile var mı kontrolü yapılır.
    private bool CheckForMatchesForOneTile(TileClass tile)
    {
        bool result = false;

        List<TileClass> neighborTiles = this.FindAllNeighbors(tile);
        List<TileClass> sameColorNeighbors = new List<TileClass>();

        //ilk başta bütün komşuları bulunup, bu komşulardan kontrolü yapılan tile ile aynı renkte olan var mı diye bakılır.
        for (int i = 0; i < neighborTiles.Count; i++)
        {
            //Debug.Log(neighborTiles[i].x + "------------" + neighborTiles[i].y);
            if (neighborTiles[i].color == tile.color)
            {
                sameColorNeighbors.Add(neighborTiles[i]);
            }
        }
        //Aynı renkte olan komşuların, birbiriyle komşu olup olmadığı bakılır, eğer birbirleriyle de komşularsa silinecek olarak işaretlenirler.
        for (int i = 0; i < sameColorNeighbors.Count; i++)
        {
            for (int k = 0; k < sameColorNeighbors.Count; k++)
            {
                //Debug.Log(sameColorNeighbors[i].x + "-----samecolor-----" + sameColorNeighbors[i].y);
                if (i != k && this.IsTwoTilesNeighbors(sameColorNeighbors[i], sameColorNeighbors[k]) && sameColorNeighbors[i].color == sameColorNeighbors[k].color)
                {
                    this.SetDeleteTiles(tile, sameColorNeighbors);
                    
                    result = true;
                }
            }
        }
        return result;
    }

    private List<TileClass> FindAllNeighbors(TileClass tile)
    {
        List<TileClass> neighborList = new List<TileClass>();
        //y değeri tek olan ve y değeri çift olan tilelar için bulma mantığı farklı
        //y değeri tek ise bir üstteki rowdan 3, kendi rowundan 2, bir alt rowdan 1 tane komşusu var.
        //y değeri çift ise bir üstteki rowdan 1 kendi rowundan 2, bir alt rowdan 3 konşusu var.
        int currentRow = tile.x;
        int currentColumn = tile.y;
        if (currentColumn % 2 != 0)
        {
            //Alt row komşusunu ekleme
            if (currentRow != 0)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn));
            }

            //Kendi rowundaki komşularını ekleme
            if (currentColumn != 0)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn - 1));
            }
            if (currentColumn != numberOfColumns.value - 1)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn + 1));
            }

            //Bir üst rowundaki komşularını ekleme
            if (currentRow != numberOfRows.value - 1)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn));

                if (currentColumn != 0)
                {
                    neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn - 1));
                }

                if (currentColumn != numberOfColumns.value - 1)
                {
                    neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn + 1));
                }
            }
        }
        else
        {
            //Alt row komşusunu ekleme
            if (currentRow != 0)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn));
                if (currentColumn != 0)
                {
                    neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn - 1));
                }

                if (currentColumn != numberOfColumns.value - 1)
                {
                    neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn + 1));
                }
            }

            //Kendi rowundaki komşularını ekleme
            if (currentColumn != 0)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn - 1));
            }
            if (currentColumn != numberOfColumns.value - 1)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn + 1));
            }

            //Bir üst rowundaki komşularını ekleme
            if (currentRow != numberOfRows.value - 1)
            {
                neighborList.Add(allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn));
            }
        }
        neighborList = neighborList.Where(tile1 => tile1 != null).ToList();
        return neighborList;
    }
    //Verilen iki tile'ın birbirine komşu olup olmadığını dönen fonksiyon
    private bool IsTwoTilesNeighbors(TileClass tile1, TileClass tile2)
    {
        List<TileClass> tile1Neighbors = this.FindAllNeighbors(tile1);
       ;
        if (tile1Neighbors.Exists(tile => tile.x == tile2.x && tile.y == tile2.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HandleTilesRotation(List<TileClass> tiles, bool isItCounterClockwise, bool useTimer)
    {
        RotateSelectedTiles(tiles, isItCounterClockwise);
        if (useTimer)
        {
            GameManager.instance.isTilesRotating = true;
            rotationTimer = totalRotationTime;
        }
    }

    //Tileların rotasyon işlemi için yazılmış fonksiyon.
    private void RotateSelectedTiles(List<TileClass> tiles, bool isItCounterClockwise)
    {
        List<TileClass> orderedTileList = new List<TileClass>();
        orderedTileList = tiles.OrderByDescending(tile => tile.x * numberOfColumns.value + tile.y).ToList();

        //Saat yönünün tersine işlem gerçekleştirilmesine göre ve en büyük tile'ın (satır ve sütün olarak), ortanca tile ile aynı satırda olup olmadığına göre, sıralamalar ve düzenlemeler yapılır rotasyon için.
        if (isItCounterClockwise)
        {
            if (orderedTileList[orderedTileList.Count - 1].x == orderedTileList[orderedTileList.Count - 2].x)
            {
                orderedTileList = tiles.OrderBy(tile => tile.x * numberOfColumns.value + tile.y).ToList();
            }
        }
        else
        {
            if (orderedTileList[orderedTileList.Count - 1].x != orderedTileList[orderedTileList.Count - 2].x)
            {
                orderedTileList = tiles.OrderBy(tile => tile.x * numberOfColumns.value + tile.y).ToList();
            }
        }

        //Düzenlemeler yapıldıktan sonra, tilelar bir ötelenir.
        int tempX = 0;
        int tempY = 0;
        for (int i = 0; i < orderedTileList.Count; i++)
        {
            //Debug.Log(orderedTileList[i].x + "--i--" + orderedTileList[i].y);
            if (i == 0)
            {
                tempX = orderedTileList[i].x;
                tempY = orderedTileList[i].y;
            }
            if (i == orderedTileList.Count - 1)
            {
                orderedTileList[i].x = tempX;
                orderedTileList[i].y = tempY;
            }
            else
            {
                orderedTileList[i].x = orderedTileList[i + 1].x;
                orderedTileList[i].y = orderedTileList[i + 1].y;
            }
        }

        DrawAndSetTilesScript.DrawTiles();
    }

    //hangi tilelların sileneceği flaglenir.
    public void SetDeleteTiles(TileClass tile, List<TileClass> sameColorTiles)
    {
        tilesToBeDeleted.Add(tile);
        tilesToBeDeleted.AddRange(sameColorTiles);
        tilesToCheck.Remove(tile);
        for (int i = 0; i < sameColorTiles.Count; i++)
        {
            tilesToCheck.RemoveAll(tile1 => tile1.x == sameColorTiles[i].x && tile1.y == sameColorTiles[i].y);
        }
        sameColorTiles.Clear();
        GameManager.instance.isTilesRotating = true;
        rotationTimer = totalRotationTime;
    }

    //Belirli bir tile'ı silmek için kullanılır.
    public void DeleteTile(TileClass tile)
    {
        allTiles.tileList.Remove(tile);
        selectedTiles.tileList.Remove(tile);
        DrawAndSetTilesScript.AddNewTileToTop(tile);

        if (tile.isItBombTile)
        {
            tile.isItBombTile = false;
            isBombActive.value = false;
        }

        score.value += scoreMultiplier;

        if (score.value % 1000 == 0)
        {
            bombScript.SpawnBomb();
        }
    }

    public List<TileClass> FindMatches()
    {
        List<TileClass> matchPositions = new List<TileClass>();

        for (int y = 0; y < numberOfColumns.value - 1; y++)
        {
            
            if (allTiles.getTile(0,y).color == allTiles.getTile(0, y + 1).color)
            {
                if (allTiles.getTile(0, y + 1).color == allTiles.getTile(1, y + 1).color)
                {
                    matchPositions.Add(allTiles.getTile(0, y));
                    matchPositions.Add(allTiles.getTile(0, y + 1));
                    matchPositions.Add(allTiles.getTile(1, y + 1));
                }
            }
        }

        for (int x = 1; x < numberOfRows.value - 1; x++)
        {
            //if (x % 2 == 0)
            //{
            for (int y = 0; y < numberOfColumns.value - 1; y++)
            {
                if (y % 2 == 0)
                {
                    if (allTiles.getTile(x, y).color == allTiles.getTile(x, y + 1).color)
                    {
                        if (allTiles.getTile(x, y + 1).color == allTiles.getTile(x + 1, y).color)
                        {
                            matchPositions.Add(allTiles.getTile(x, y));
                            matchPositions.Add(allTiles.getTile(x, y + 1));
                            matchPositions.Add(allTiles.getTile(x + 1, y));
                        }
                        if (allTiles.getTile(x, y + 1).color == allTiles.getTile(x - 1, y + 1).color)
                        {
                            matchPositions.Add(allTiles.getTile(x, y));
                            matchPositions.Add(allTiles.getTile(x, y + 1));
                            matchPositions.Add(allTiles.getTile(x - 1, y + 1));
                        }
                    }
                }
                else
                {
                    if (allTiles.getTile(x, y).color == allTiles.getTile(x, y + 1).color)
                    {

                        if (allTiles.getTile(x + 1, y + 1).color == allTiles.getTile(x , y + 1).color)
                        {
                            matchPositions.Add(allTiles.getTile(x, y));
                            matchPositions.Add(allTiles.getTile(x, y + 1));
                            matchPositions.Add(allTiles.getTile(x + 1, y + 1));
                        }
                        if (allTiles.getTile(x - 1, y).color == allTiles.getTile(x, y + 1).color)
                        {
                            matchPositions.Add(allTiles.getTile(x, y));
                            matchPositions.Add(allTiles.getTile(x, y + 1));
                            matchPositions.Add(allTiles.getTile(x - 1, y));
                        }
                    }
                }
            }
            //}
            //else
            //{
            //    for (int y = 0; y < numberOfColumns.value - 1; y++)
            //    {
            //        if (allTiles.getTile(x, y).color == allTiles.getTile(x, y + 1).color)
            //        {

            //            if (allTiles.getTile(x + 1, y + 1).color == allTiles.getTile(x + 1, y).color)
            //            {
            //                matchPositions.Add(allTiles.getTile(x, y));
            //                matchPositions.Add(allTiles.getTile(x, y + 1));
            //                matchPositions.Add(allTiles.getTile(x + 1, y + 1));
            //            }
            //            if (allTiles.getTile(x - 1, y).color == allTiles.getTile(x, y + 1).color)
            //            {
            //                matchPositions.Add(allTiles.getTile(x, y));
            //                matchPositions.Add(allTiles.getTile(x, y + 1));
            //                matchPositions.Add(allTiles.getTile(x - 1, y));
            //            }
            //        }
            //    }
            //}
        }

        for (int y = 0; y < numberOfColumns.value - 1; y++)
        {

            if (allTiles.getTile(numberOfRows.value - 1, y).color == allTiles.getTile(numberOfRows.value - 1, y + 1).color)
            {
                if (allTiles.getTile(numberOfRows.value - 1, y + 1).color == allTiles.getTile(numberOfRows.value - 2,  y).color)
                {
                    matchPositions.Add(allTiles.getTile(numberOfRows.value - 1, y));
                    matchPositions.Add(allTiles.getTile(numberOfRows.value - 1, y + 1));
                    matchPositions.Add(allTiles.getTile(numberOfRows.value - 2, y));
                }
            }
        }

        matchPositions = matchPositions
            .GroupBy(a => new { a.x, a.y })
            .Select(b => b.First())
            .ToList();
        return matchPositions;
    }

    private void DeleteTiles(List<TileClass> tilesToBeDeleted)
    {
        List<TileClass> tilesToBeDeletedAfterDraw = new List<TileClass>();
        for (int i = 0; i < tilesToBeDeleted.Count; i++)
        {
            this.DeleteTile(tilesToBeDeleted[i]);
        }
        tilesToBeDeleted.Clear();
        DrawAndSetTilesScript.DrawTiles();

        tilesToBeDeletedAfterDraw = this.FindMatches();

        if (tilesToBeDeletedAfterDraw.Count > 0)
        {
            this.DeleteTiles(tilesToBeDeletedAfterDraw);
        }
    }
}
