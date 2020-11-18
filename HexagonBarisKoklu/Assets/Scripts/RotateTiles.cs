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
    public Tilemap tilemap;
    public GameObject debugPoint;

    public GameObject rotatingTiles;

    public float totalRotationTime = 2.0f;
    public int scoreMultiplier = 5;

    [NonSerialized] public bool isRotatingCounterClockwise;

    private BombScript bombScript;
    private DrawAndSetTiles DrawAndSetTilesScript;
    private bool isRotationComplete = false;
    private bool isRotatingBackwards = false;
    private float rotationTimer = 0.0f;
    private List<TileClass> tilesToBeDeleted = new List<TileClass>();
    private List<TileClass> tilesToCheck = new List<TileClass>();
    private Vector3 cellToWorld1;
    private Vector3 cellToWorld2;
    private Vector3 celltoWorld3;
    private Vector3 middlePoint;
    // Start is called before the first frame update
    void Start()
    {
        bombScript = gameObject.GetComponent<BombScript>();
        DrawAndSetTilesScript = gameObject.GetComponent<DrawAndSetTiles>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.currentGameState == GameManager.GameStates.PausedState)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.K) && selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
        {
            isRotatingCounterClockwise = false;
            this.SetTileRotationObjectsAndAnimate();
            this.HandleTilesRotation(selectedTiles.tileList, isRotatingCounterClockwise, true);//bu fonksiyon animasyon yapıldıktan sonra animasyon bitişinde çağırılacak şeklinde ayarlanmalıdır.
        }

        if (Input.GetKeyDown(KeyCode.Y) && selectedTiles.tileList.Count != 0 && !GameManager.instance.isTilesRotating)
        {
            isRotatingCounterClockwise = true;
            this.SetTileRotationObjectsAndAnimate();
            this.HandleTilesRotation(selectedTiles.tileList, isRotatingCounterClockwise, true);//bu fonksiyon animasyon yapıldıktan sonra animasyon bitişinde çağırılacak şeklinde ayarlanmalıdır.
        }


        if (GameManager.instance.isTilesRotating)
        {
            this.HandleRotation();
        }
    }

    private void SetTileRotationObjectsAndAnimate()
    {
        //rotatingTiles.SetActive(true);

        for (int i = 0; i < rotatingTiles.transform.childCount; i++)
        {
            Transform currentChild = rotatingTiles.transform.GetChild(i);
            Vector3 worldPos = tilemap.CellToWorld(new Vector3Int(selectedTiles.tileList[i].x, selectedTiles.tileList[i].y, 1));
            currentChild.position = worldPos;
            currentChild.gameObject.GetComponent<SpriteRenderer>().color = selectedTiles.tileList[i].color;
        }
    }

    private void HandleRotation()
    {
        //isTileRotating durumu oyuncu bir tile seçtikten sonra bu tilelı sağa veya sola döndürme durumu gerçekleşmekteyse true durumdadır.
        this.RotateSelectedTiles(isRotatingCounterClockwise);

        if (rotationTimer > 0)
        {
            rotationTimer -= Time.deltaTime;
        }
        else
        {
            rotationTimer = 0.0f;
            isRotationComplete = true;
        }
        //Rotasyon yeni bitmişse bu kod çalıştırılır.

        //en son burada kalındı. 2. rotasyon süresince de oyunun duraklatılması ve o rotasyonun gerçekleştirilmesi lazım.
        if (isRotationComplete)
        {
            //Rotasyon bittiği zaman, tilelar arasında aynı renk olan eşleşen tilelar var mı kontrolünün yapıldığı yer.
            tilesToBeDeleted = this.FindMatches();
            //Eğer eşleşen (yani yok edilecek) tile olduysa o tileları yoketme işlemi ve eşleyen hiçbir tile olmadıysa tileları eski halina döndürme işlemi burada yapılır.
            if (tilesToBeDeleted.Count == 0)
            {
                isRotatingCounterClockwise = !isRotatingCounterClockwise;
                this.HandleTilesRotation(selectedTiles.tileList, isRotatingCounterClockwise, true);
                isRotatingBackwards = !isRotatingBackwards;
            }
            else
            {
                this.DeleteTiles(tilesToBeDeleted);
                GameManager.instance.isTilesRotating = false;
                
            }
            bombScript.CheckForBomb();
            isRotationComplete = false;
            if (isRotatingBackwards)
            {
                GameManager.instance.isTilesRotating = false;
            }
        }
    }

    public void HandleTilesRotation(List<TileClass> tiles, bool isItCounterClockwise, bool useTimer)
    {
        RotateSelectedTiles(tiles, isItCounterClockwise);
        if (useTimer)
        {
            GameManager.instance.isTilesRotating = true;
            rotationTimer = totalRotationTime;
            cellToWorld1 = tilemap.GetCellCenterWorld(new Vector3Int(selectedTiles.tileList[0].x, selectedTiles.tileList[0].y, 1));
            cellToWorld2 = tilemap.GetCellCenterWorld(new Vector3Int(selectedTiles.tileList[1].x, selectedTiles.tileList[1].y, 1));
            celltoWorld3 = tilemap.GetCellCenterWorld(new Vector3Int(selectedTiles.tileList[2].x, selectedTiles.tileList[2].y, 1));
            middlePoint = new Vector3((cellToWorld1.x + cellToWorld2.x + celltoWorld3.x) / 3,
                                         (cellToWorld1.y + cellToWorld2.y + celltoWorld3.y) / 3, 1);
            debugPoint.transform.position = middlePoint;

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

            if (allTiles.getTile(0, y).color == allTiles.getTile(0, y + 1).color)
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

                        if (allTiles.getTile(x + 1, y + 1).color == allTiles.getTile(x, y + 1).color)
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
                if (allTiles.getTile(numberOfRows.value - 1, y + 1).color == allTiles.getTile(numberOfRows.value - 2, y).color)
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

    private void RotateSelectedTiles(bool clockwise)
    {
        for (int i = 0; i < rotatingTiles.transform.childCount; i++)
        {
            Transform currentChild = rotatingTiles.transform.GetChild(i);
            currentChild.RotateAround(middlePoint, Vector3.forward * (isRotatingCounterClockwise ? 1 : -1), 60 * Time.deltaTime);
            //Quaternion toAngle = Quaternion.LookRotation(middlePoint - currentChild.position);
            //currentChild.rotation = Quaternion.Slerp(currentChild.rotation, toAngle, 10f * Time.deltaTime);
        }
    }
}
