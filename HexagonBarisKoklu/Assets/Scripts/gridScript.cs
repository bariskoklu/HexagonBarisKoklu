using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class gridScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Tile exampleTile;
    public int numberOfRows = 8;
    public int numberOfColumns = 9;
    public List<Color> colors;
    //public TileClass[,] tiles;
    public List<TileClass> allTiles = new List<TileClass>();
    public List<TileClass> selectedTiles = new List<TileClass>();
    private Tilemap tilemap;

    private void Awake()
    {
        //tiles = new TileClass[numberOfRows, numberOfColumns];
    }
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();

        for (int k = 0; k < numberOfRows; k++)
        {
            for (int i = 0; i < numberOfColumns; i++)
            {
                Color color = colors[UnityEngine.Random.Range(0, colors.Count)];
                allTiles.Add(new TileClass(color,k,i));
                tilemap.SetTile(new Vector3Int(k, i, 1), exampleTile);
                tilemap.SetTileFlags(new Vector3Int(k, i, 1), TileFlags.None);
                tilemap.SetColor(new Vector3Int(k, i, 1), color);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GetThreeClosestTiles();
            Vector3Int deneme;
            deneme = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log(tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            
            TileClass a = (allTiles.FirstOrDefault(tileBelow => tileBelow.x == deneme.x && tileBelow.y == deneme.y));
            this.CheckForMatchesForOneTile(a);

        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            this.RotateSelectedTilesToRight(selectedTiles);
        }
    }

    //Basılan noktaya en yakın 3 tile bizim için seçilen 3 tile olacak. Üçgen paterni bu şekilde karşılanıyor.
    private void GetThreeClosestTiles()
    {
        Vector3 mouseposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        TileClass furthestTile = new TileClass(Color.white, 0, 0);
        //Bütün satır ve sütunları dönüyoruz
        selectedTiles.Clear();
        for (int k = 0; k < numberOfRows; k++)
        {
            for (int i = 0; i < numberOfColumns; i++)
            {
                TileClass currentTile = allTiles.FirstOrDefault(tile => tile.x == k && tile.y == i);
                Vector3Int currentTilePosition = new Vector3Int(k, i, 1);
                if (k == 0 && i < 3)
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

    private float CalculateDistanceBetweenTileAndWorldPos(TileClass tile, Vector3 worldPos)
    {
        float distance  = 0.0f;

        Vector3Int tilePosition = new Vector3Int(tile.x, tile.y, 1);
        Vector2 tileWorldSpacePosition = tilemap.CellToWorld(tilePosition);
        distance = Vector2.Distance(tileWorldSpacePosition, worldPos);

        return distance;
    }

    private void CheckForMatchesForOneTile(TileClass tile)
    {
        List<TileClass> neighborTiles = this.FindAllNeighbors(tile);
        List<TileClass> sameColorNeighbors = new List<TileClass>();
        for (int i = 0; i < neighborTiles.Count; i++)
        {
            Debug.Log(neighborTiles[i].x + "------------" + neighborTiles[i].y);
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
                    //TO DO: tileların yokolması ve yokolan rowlar için yeni tiller gelmesi sağlanacak.
                    //Debug.Log("Tiles Matched");
                }
            }
        }
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
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn));
            }

            //Kendi rowundaki komşularını ekleme
            if (currentColumn != 0)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn - 1));
            }
            if (currentColumn != numberOfColumns -1)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn + 1));
            }
            
            //Bir üst rowundaki komşularını ekleme
            if (currentRow != numberOfRows -1)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn));

                if (currentColumn != 0)
                {
                    neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn - 1));
                }

                if (currentColumn != numberOfColumns - 1)
                {
                    neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn + 1));
                }
            }
        }
        else
        {
            //Alt row komşusunu ekleme
            if (currentRow != 0)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn));
                if (currentColumn != 0)
                {
                    neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn - 1));
                }

                if (currentColumn != numberOfColumns - 1)
                {
                    neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow - 1 && tileBelow.y == currentColumn + 1));
                }
            }

            //Kendi rowundaki komşularını ekleme
            if (currentColumn != 0)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn - 1));
            }
            if (currentColumn != numberOfColumns - 1)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow && tileBelow.y == currentColumn + 1));
            }

            //Bir üst rowundaki komşularını ekleme
            if (currentRow != numberOfRows - 1)
            {
                neighborList.Add(allTiles.FirstOrDefault(tileBelow => tileBelow.x == currentRow + 1 && tileBelow.y == currentColumn));
            }
        }
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

    private void RotateSelectedTilesToRight(List<TileClass> tiles)
    {
        List<TileClass> orderedTileList = new List<TileClass>();
        orderedTileList = tiles.OrderByDescending(tile => tile.x * numberOfColumns + tile.y).ToList();
        for (int i = 0; i < orderedTileList.Count; i++)
        {
            Debug.Log(orderedTileList[i].x + "--i--" + orderedTileList[i].y);


        }
    }
}
