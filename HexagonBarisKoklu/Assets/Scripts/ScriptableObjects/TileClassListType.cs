using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class TileClassListType : ScriptableObject
{
    public List<TileClass> tileList = new List<TileClass>();

    public TileClass getTile(int x, int y)
    {
        TileClass tileToReturn = tileList.FirstOrDefault(tileBelow => tileBelow.x == x && tileBelow.y == y);

        return tileToReturn;
    }
}
