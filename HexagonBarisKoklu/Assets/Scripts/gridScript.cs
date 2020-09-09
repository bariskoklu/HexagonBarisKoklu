using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class gridScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Tile tilePrefab;
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public IntType score;
    public BoolType isBombActive;
    public TileClassListType allTiles;
    public List<Color> colors;
    public List<TileClass> selectedTiles = new List<TileClass>();

    public int scoreMultiplier = 5;

    private Tilemap tilemap;
    private BombScript bombScript;
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
        bombScript = gameObject.GetComponent<BombScript>();

        this.SetTiles();
        this.DrawTiles();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GetThreeClosestTiles();
            Vector3Int deneme;
            deneme = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            //Debug.Log(tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            
            TileClass a = (allTiles.tileList.FirstOrDefault(tileBelow => tileBelow.x == deneme.x && tileBelow.y == deneme.y));

        }

        if (Input.GetKeyDown(KeyCode.K) && selectedTiles.Count != 0)
        {
            this.PerformRotation(false);
        }

        if (Input.GetKeyDown(KeyCode.Y) && selectedTiles.Count != 0)
        {
            this.PerformRotation(true);
        }
    }

    private void PerformRotation(bool isItCounterClockwise)
    {
        this.RotateSelectedTiles(selectedTiles, isItCounterClockwise);
        bool result = false;
        for (int i = 0; i < selectedTiles.Count; i++)
        {
            if (this.CheckForMatchesForOneTile(selectedTiles[i]))
            {
                result = true;
            }
        }

        if (result)
        {
            this.DrawTiles();
        }
        else
        {
            this.RotateSelectedTiles(selectedTiles, !isItCounterClockwise);
        }
    }

    private void SetTiles()
    {
        for (int k = 0; k < numberOfRows.value; k++)
        {
            for (int i = 0; i < numberOfColumns.value; i++)
            {
                Color color = colors[UnityEngine.Random.Range(0, colors.Count)];
                allTiles.tileList.Add(new TileClass(color, k, i));
            }
        }
    }

    private void DrawTiles()
    {
        selectedTiles.Clear();
        for (int k = 0; k < numberOfRows.value; k++)
        {
            for (int i = 0; i < numberOfColumns.value; i++)
            {
                TileClass currentTile = allTiles.tileList.FirstOrDefault(tile => tile.x == k && tile.y == i);
                if (currentTile != null)
                {
                    Vector3Int tilePosition = new Vector3Int(currentTile.x, currentTile.y, 1);
                    tilemap.SetTile(tilePosition, tilePrefab);
                    tilemap.SetTileFlags(tilePosition, TileFlags.None);
                    tilemap.SetColor(tilePosition, currentTile.color);
                    
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(k, i, 1), null);
                }
            }
        }
    }
    //Basılan noktaya en yakın 3 tile bizim için seçilen 3 tile olacak. Üçgen paterni bu şekilde karşılanıyor.
    private void GetThreeClosestTiles()
    {
        Vector3 mouseposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        TileClass furthestTile = new TileClass(Color.white, 0, 0);
        //Bütün satır ve sütunları dönüyoruz
        selectedTiles.Clear();
        for (int k = 0; k < numberOfRows.value; k++)
        {
            for (int i = 0; i < numberOfColumns.value; i++)
            {
                TileClass currentTile = allTiles.tileList.FirstOrDefault(tile => tile.x == k && tile.y == i);
                Vector3Int currentTilePosition = new Vector3Int(k, i, 1);
                if (currentTile != null)
                {
                    if (selectedTiles.Count < 3)
                    {
                        selectedTiles.Add(currentTile);
                    }
                    else
                    {

                        for (int x = 0; x < selectedTiles.Count; x++)
                        {

                            float distanceBetweenMouseAndFurthestTile = 0;
                            if (x == 0)
                            {
                                furthestTile = currentTile;
                            }
                            distanceBetweenMouseAndFurthestTile = CalculateDistanceBetweenTileAndWorldPos(furthestTile, mouseposition);
                            TileClass tileToReplace = selectedTiles[x];
                            float distanceBetweenMouseAndTileToReplace = CalculateDistanceBetweenTileAndWorldPos(tileToReplace, mouseposition); ;

                            if (distanceBetweenMouseAndFurthestTile < distanceBetweenMouseAndTileToReplace)
                            {
                                TileClass tempTile = selectedTiles[x];
                                selectedTiles.RemoveAt(x);
                                selectedTiles.Add(furthestTile);
                                furthestTile = tempTile;
                            }
                        }
                    }
                }
            }
        }    
    }

    private float CalculateDistanceBetweenTileAndWorldPos(TileClass tile, Vector3 worldPos)
    {
        float distance  = 0.0f;

        Vector3Int tilePosition = new Vector3Int(tile.x, tile.y, 1);
        Vector2 tileWorldSpacePosition = tilemap.CellToWorld(tilePosition);
        distance = Vector2.Distance(tileWorldSpacePosition, worldPos);

        return distance;
    }

    private bool CheckForMatchesForOneTile(TileClass tile)
    {
        bool result = false;

        List<TileClass> neighborTiles = this.FindAllNeighbors(tile);
        List<TileClass> sameColorNeighbors = new List<TileClass>();
        for (int i = 0; i < neighborTiles.Count; i++)
        {
            //Debug.Log(neighborTiles[i].x + "------------" + neighborTiles[i].y);
            if (neighborTiles[i].color == tile.color)
            {
                sameColorNeighbors.Add(neighborTiles[i]);
            }
        }
        for (int i = 0; i < sameColorNeighbors.Count; i++)
        {
            for (int k = 0; k < sameColorNeighbors.Count; k++)
            {
                //Debug.Log(sameColorNeighbors[i].x + "-----samecolor-----" + sameColorNeighbors[i].y);
                if (i != k && this.IsTwoTilesNeighbors(sameColorNeighbors[i], sameColorNeighbors[k]) && sameColorNeighbors[i].color == sameColorNeighbors[k].color)
                {
                    this.DeleteTiles(tile, sameColorNeighbors);

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
        if (math.abs(tile1.x - tile2.x) <= 1 && math.abs(tile1.y - tile2.y) <= 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void RotateSelectedTiles(List<TileClass> tiles, bool isItCounterClockwise)
    {
        List<TileClass> orderedTileList = new List<TileClass>();
        orderedTileList = tiles.OrderByDescending(tile => tile.x * numberOfColumns.value + tile.y).ToList();
        if (isItCounterClockwise)
        {
            if (orderedTileList[orderedTileList.Count-1].x == orderedTileList[orderedTileList.Count - 2].x)
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
            if (i == orderedTileList.Count -1)
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
    }

    private void DeleteTiles(TileClass tile, List<TileClass> sameColorTiles)
    {
        allTiles.tileList = allTiles.tileList.Except(sameColorTiles).ToList();
        allTiles.tileList.Remove(tile);
        selectedTiles = selectedTiles.Except(sameColorTiles).ToList();
        selectedTiles.Remove(tile);

        for (int a = 0; a < sameColorTiles.Count; a++)
        {

            if (sameColorTiles[a].isItBombTile)
            {
                sameColorTiles[a].isItBombTile = false;
                isBombActive.value = false;
            }

            score.value += scoreMultiplier;

            if (score.value % 1000 == 0)
            {
                bombScript.SpawnBomb();
            }
        }

        sameColorTiles.Clear();
    }
}
