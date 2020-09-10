using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawAndSetTiles : MonoBehaviour
{
    // Start is called before the first frame update
    public Tile tilePrefab;
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public TileClassListType allTiles;
    public TileClassListType selectedTiles;
    public List<Color> colors;
    

    private Tilemap tilemap;
    private BombScript bombScript;
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();
        bombScript = gameObject.GetComponent<BombScript>();

        this.SetTiles();
        this.DrawTiles();
    }
    //Her kolon ve row için yeni bir tileclass tipinde instance oluşturur ve bunları allTiles scriptable objesine atar.
    public void SetTiles()
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

    //allTilesta bulunan objeler için tilemape yeni tile ataması yapar.
    public void DrawTiles()
    {
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

    //Bir tile yokedilince, bu tileın üstündeki diğer tileları bir alta indirip, en üste yeni bir tile yaratır.
    public void AddNewTileToTop(TileClass DestroyedTile)
    {
        int tileX = DestroyedTile.x;
        int tileY = DestroyedTile.y;
        List<TileClass> tilesToGoDown = allTiles.tileList.Where(tile => tile.x > tileX && tile.y == tileY).ToList();

        for (int i = 0; i < tilesToGoDown.Count; i++)
        {
            tilesToGoDown[i].x--;
        }
        Color color = colors[UnityEngine.Random.Range(0, colors.Count)];
        TileClass tileToAdd = new TileClass(color, numberOfRows.value -1, tileY);
        allTiles.tileList.Add(tileToAdd);
    }
}
