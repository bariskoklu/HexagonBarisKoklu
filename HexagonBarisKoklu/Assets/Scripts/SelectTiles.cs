using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Tilemaps;

public class SelectTiles : MonoBehaviour
{
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public IntType score;
    public TileClassListType allTiles;
    public TileClassListType selectedTiles;

    private Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.currentGameState == GameManager.GameStates.PausedState)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            this.GetThreeClosestTiles();
        }
    }

    //Basılan noktaya en yakın 3 tile bizim için seçilen 3 tile olacak. Üçgen paterni bu şekilde karşılanıyor.
    public void GetThreeClosestTiles()
    {
        Vector3 mouseposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        TileClass furthestTile = new TileClass(Color.white, 0, 0);
        selectedTiles.tileList.Clear();
        TileClass mousepositionTile = this.GetTileOnThePosition(mouseposition);

        if (mousepositionTile != null)
        {
            Vector3Int mousepositionTileVector = new Vector3Int(mousepositionTile.x, mousepositionTile.y, 0);
            //seçilen tile'ın uç tilelarda olması durumunda yanlış seçimi önlemek için yazılmıştır.
            //Eğer en üstteyse mousepozisyonunun y sini cell'in ortasındaki y pozisyonundan 1 pixel aşağı sabitleyeceğiz. En alttaysa y'sini 1 pixel yukarı sabitleyeceğiz.
            //En sağda ve solda durumları için de x pozisyonun sırasıyla ortadan bir sol ve bir sağ olarak sabitleyeceğiz.
            if (mousepositionTile.y == numberOfColumns.value -1)
            {
                mouseposition.x = tilemap.GetCellCenterWorld(mousepositionTileVector).x - 0.1f;
            }
            else if (mousepositionTile.y == 0)
            {
                mouseposition.x = tilemap.GetCellCenterWorld(mousepositionTileVector).x + 0.1f;
            }
            if (mousepositionTile.x == numberOfRows.value - 1)
            {
                mouseposition.y = tilemap.GetCellCenterWorld(mousepositionTileVector).y - 0.1f;
            }
            else if (mousepositionTile.x == 0)
            {
                mouseposition.y = tilemap.GetCellCenterWorld(mousepositionTileVector).y + 0.1f;
            }


            //Bütün satır ve sütunları dönüyoruz
            for (int k = 0; k < numberOfRows.value; k++)
            {
                for (int i = 0; i < numberOfColumns.value; i++)
                {
                    TileClass currentTile = allTiles.getTile(k, i);
                    Vector3Int currentTilePosition = new Vector3Int(k, i, 1);
                    if (currentTile != null)
                    {
                        if (selectedTiles.tileList.Count < 3)
                        {
                            selectedTiles.tileList.Add(currentTile);
                        }
                        else
                        {
                            furthestTile = currentTile;

                            for (int x = 0; x < selectedTiles.tileList.Count; x++)
                            {
                                float distanceBetweenMouseAndFurthestTile = 0;
                                distanceBetweenMouseAndFurthestTile = CalculateDistanceBetweenTileAndWorldPos(furthestTile, mouseposition);
                                TileClass tileToReplace = selectedTiles.tileList[x];
                                float distanceBetweenMouseAndTileToReplace = CalculateDistanceBetweenTileAndWorldPos(tileToReplace, mouseposition); ;

                                if (distanceBetweenMouseAndFurthestTile <= distanceBetweenMouseAndTileToReplace)
                                {
                                    TileClass tempTile = selectedTiles.tileList[x];
                                    selectedTiles.tileList.RemoveAt(x);
                                    selectedTiles.tileList.Add(furthestTile);
                                    furthestTile = tempTile;
                                    x = 0; //for resetting the loop
                                }
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Mouse Position : " + mouseposition);
        for (int x = 0; x < selectedTiles.tileList.Count; x++)
        {
            Vector3Int tilePosition = new Vector3Int(selectedTiles.tileList[x].x, selectedTiles.tileList[x].y, 0);
            Debug.Log("Cell " + x + " Position : " + tilemap.GetCellCenterWorld(tilePosition));
        }
        
    }

    private float CalculateDistanceBetweenTileAndWorldPos(TileClass tile, Vector3 worldPos)
    {
        float distance = 0.0f;

        Vector3Int tilePosition = new Vector3Int(tile.x, tile.y, -10);
        Vector3 tileWorldSpacePosition = tilemap.GetCellCenterWorld(tilePosition);
        distance = Vector2.Distance(tileWorldSpacePosition, worldPos);

        return distance;
    }

    private TileClass GetTileOnThePosition(Vector3 position)
    {

        Vector3Int cellPosition = tilemap.WorldToCell(position);

        if (allTiles.tileList.Exists(tileToFind => tileToFind.x == cellPosition.x && tileToFind.y == cellPosition.y))
        {
            return allTiles.getTile(cellPosition.x, cellPosition.y);
        }
        else
        {
            return null;
        }
    }
}
