using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportNetworkTile 
{
    public Type type = Type.Empty;
    public Vector2Int position = new Vector2Int();

    public TransportNetworkTile()
    {
        position.x = -1;
        position.y = -1; 
    }

    public TransportNetworkTile(int positionX, int positionY)
    {
        position.x = positionX;
        position.y = positionY; 
    }

    public enum Type
    {
        Empty, 
        Road
    }
}
