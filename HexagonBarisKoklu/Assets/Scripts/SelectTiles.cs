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
        //Bütün satır ve sütunları dönüyoruz
        selectedTiles.tileList.Clear();
        for (int k = 0; k < numberOfRows.value; k++)
        {
            for (int i = 0; i < numberOfColumns.value; i++)
            {
                TileClass currentTile = allTiles.tileList.FirstOrDefault(tile => tile.x == k && tile.y == i);
                Vector3Int currentTilePosition = new Vector3Int(k, i, 1);
                if (currentTile != null)
                {
                    if (selectedTiles.tileList.Count < 3)
                    {
                        selectedTiles.tileList.Add(currentTile);
                    }
                    else
                    {

                        for (int x = 0; x < selectedTiles.tileList.Count; x++)
                        {

                            float distanceBetweenMouseAndFurthestTile = 0;
                            if (x == 0)
                            {
                                furthestTile = currentTile;
                            }
                            distanceBetweenMouseAndFurthestTile = CalculateDistanceBetweenTileAndWorldPos(furthestTile, mouseposition);
                            TileClass tileToReplace = selectedTiles.tileList[x];
                            float distanceBetweenMouseAndTileToReplace = CalculateDistanceBetweenTileAndWorldPos(tileToReplace, mouseposition); ;

                            if (distanceBetweenMouseAndFurthestTile < distanceBetweenMouseAndTileToReplace)
                            {
                                TileClass tempTile = selectedTiles.tileList[x];
                                selectedTiles.tileList.RemoveAt(x);
                                selectedTiles.tileList.Add(furthestTile);
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
        float distance = 0.0f;

        Vector3Int tilePosition = new Vector3Int(tile.x, tile.y, 1);
        Vector2 tileWorldSpacePosition = tilemap.CellToWorld(tilePosition);
        distance = Vector2.Distance(tileWorldSpacePosition, worldPos);

        return distance;
    }
}
