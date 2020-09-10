using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DrawBombText : MonoBehaviour
{
    public IntType numberOfRows;
    public IntType numberOfColumns;
    public IntType score;
    public BoolType isBombActive;
    public TileClassListType allTiles;
    public IntType bombActionCount;
    public Tilemap tileMap;
    public Text text;

    private TileClass bombTile = new TileClass(Color.white,0, 0);
    private Vector3 bombTileLocalPosiiton;


    void Update()
    {
        //Bomba aktifse, bombanın aktif olduğunu ve kaç move kaldığını belirten bir texti aktif ederek textin içini kalan move sayısıyla doldurur.
        if (isBombActive)
        {
            gameObject.SetActive(true);
            bombTile = allTiles.tileList.FirstOrDefault(tile => tile.isItBombTile == true);
            if (bombTile != null)
            {
                bombTileLocalPosiiton = tileMap.CellToWorld(new Vector3Int(bombTile.x, bombTile.y, 1));
                gameObject.transform.position = bombTileLocalPosiiton;

                text.text = bombActionCount.value.ToString();
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
        
    }
}
