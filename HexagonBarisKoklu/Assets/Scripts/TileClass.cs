using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileClass
{
    public Color color;
    public int x;
    public int y;
    public bool isItBombTile = false;
    public TileClass(Color colorParameter, int xParameter, int yParameter){

        color = colorParameter;
        x = xParameter;
        y = yParameter;

    }
}
