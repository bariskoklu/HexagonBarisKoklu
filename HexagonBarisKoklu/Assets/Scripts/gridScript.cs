using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class gridScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Tile exampleTile;
    public int numberOfRows = 8;
    public int numberOfColumns = 9;
    public List<Color> colors;
    public TileClass[,] tiles;
    private Tilemap tilemap;

    private void Awake()
    {
        tiles = new TileClass[numberOfColumns, numberOfRows];
    }
    void Start()
    {
        tilemap = gameObject.GetComponent<Tilemap>();

        for (int i = 0; i < numberOfRows; i++)
        {
            for (int k = 0; k < numberOfColumns; k++)
            {
                Color color = colors[Random.Range(0, colors.Count)];
                tiles[k, i] = new TileClass(color,k,i);
                tilemap.SetTile(new Vector3Int(k, i, 1), exampleTile);
                tilemap.SetTileFlags(new Vector3Int(k, i, 1), TileFlags.None);
                tilemap.SetColor(new Vector3Int(k, i, 1), color);
                Debug.Log(tiles[k, i].color);
            }
        }

        //tilemap.SetTile(new Vector3Int(1, 1, 1), exampleTile);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log(tilemap.WorldToCell(tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition))));

        }
    }
}
