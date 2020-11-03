using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DrawSelectedTile : MonoBehaviour
{
    public TileClassListType selectedTiles;
    public Tilemap tilemap;

    private List<GameObject> selectedTileImages = new List<GameObject>();
    private void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            selectedTileImages.Add(gameObject.transform.GetChild(i).gameObject);
        }
    }

    
    void Update()
    {
        if (GameManager.instance.currentGameState == GameManager.GameStates.PausedState)
        {
            return;
        }
        //SelectedTile listi için, 3 tane varolan image'ı dolrurur ve world pozisyonlarını bu listenin içindeki tileların world pozisyonlarıyla doldurur.
        if (selectedTiles.tileList.Count == 3)
        {
            for (int i = 0; i < selectedTileImages.Count; i++)
            {
                selectedTileImages[i].SetActive(true);
                TileClass selectedTile = selectedTiles.tileList[i];
                Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(selectedTile.x, selectedTile.y, 1));
                selectedTileImages[i].transform.position = worldPos;
            }
        }
        else
        {
            for (int i = 0; i < selectedTileImages.Count; i++)
            {
                selectedTileImages[i].SetActive(false);
            }
        }
    }
}
