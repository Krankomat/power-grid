using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportNetworkManager : MonoBehaviour
{
    public float tileWidth = 1.0f;
    public float tileHeight = 1.0f; 
    public ushort tileMapWidth;
    public ushort tileMapHeight; 

    // Tilemap starts at bottom left (Unity: X: 0, Z: 0) and goes to top right (Unity: X: +Infinity, Z: +Infinity) 
    TransportNetworkTile[,] tiles;

    private void Awake()
    {
        tiles = new TransportNetworkTile[tileMapWidth, tileMapHeight];
        for (int i = 0; i < tileMapWidth; i++)
            for (int j = 0; j < tileMapHeight; j++)
                tiles[i, j] = new TransportNetworkTile(i, j); 
    }


    public void AddRoadAt(Vector2Int position)
    {
        TransportNetworkTile tile = tiles[position.x, position.y];
        if (tile.type == TransportNetworkTile.Type.Road)
            Debug.LogError($"ERROR ADDING ROAD TILE: There already is a road at X: {position.x}, Y: {position.y}. ");
        tile.type = TransportNetworkTile.Type.Road;
    }


    public void AddRoadAt(int positionX, int positionY)
    {
        AddRoadAt(new Vector2Int(positionX, positionY)); 
    }


    public void ClearTileAt(Vector2Int position)
    {
        TransportNetworkTile tile = tiles[position.x, position.y];
        if (tile.type == TransportNetworkTile.Type.Empty)
            Debug.LogError($"ERROR CLEARING TILE: There is nothing to clear at X: {position.x}, Y: {position.y}. ");
        tile.type = TransportNetworkTile.Type.Empty;
    }


    public void ClearTileAt(int positionX, int positionY)
    {
        ClearTileAt(new Vector2Int(positionX, positionY));
    }


    private List<TransportNetworkTile> GetNeighbors(TransportNetworkTile tile)
    {
        List<TransportNetworkTile> neighbors = new List<TransportNetworkTile>();
        TransportNetworkTile neighborTop    = GetNeighborTop(tile); 
        TransportNetworkTile neighborBottom = GetNeighborBottom(tile); 
        TransportNetworkTile neighborLeft   = GetNeighborLeft(tile); 
        TransportNetworkTile neighborRight  = GetNeighborRight(tile);
        if (neighborTop != null)
            neighbors.Add(neighborTop);
        if (neighborBottom != null)
            neighbors.Add(neighborBottom);
        if (neighborLeft != null)
            neighbors.Add(neighborLeft);
        if (neighborRight != null)
            neighbors.Add(neighborRight);
        return neighbors; 
    }

    private TransportNetworkTile GetNeighborTop(TransportNetworkTile tile)
    {
        if (TileIsAtTopEdge(tile))
            return null;
        return tiles[tile.position.x, tile.position.y + 1];
    }

    private TransportNetworkTile GetNeighborBottom(TransportNetworkTile tile)
    {
        if (TileIsAtBottomEdge(tile))
            return null;
        return tiles[tile.position.x, tile.position.y - 1];
    }

    private TransportNetworkTile GetNeighborLeft(TransportNetworkTile tile)
    {
        if (TileIsAtLeftEdge(tile))
            return null;
        return tiles[tile.position.x - 1, tile.position.y];
    }

    private TransportNetworkTile GetNeighborRight(TransportNetworkTile tile)
    {
        if (TileIsAtRightEdge(tile))
            return null;
        return tiles[tile.position.x + 1, tile.position.y];
    }

    private bool TileIsAtTopEdge(TransportNetworkTile tile)
    {
        int positionY = tile.position.y; 
        if (positionY > tileMapHeight - 1)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.y = {positionY}, " +
                $"but the tile map only has a height of {tileMapHeight}. "); 
        return positionY >= tileMapHeight - 1;
    }

    private bool TileIsAtBottomEdge(TransportNetworkTile tile)
    {
        int positionY = tile.position.y;
        if (positionY < 0)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.y = {positionY}, " +
                $"that is outside of the tile map. ");
        return positionY <= 0;
    }

    private bool TileIsAtLeftEdge(TransportNetworkTile tile)
    {
        int positionX = tile.position.x;
        if (positionX < 0)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.x = {positionX}, " +
                $"that is outside of the tile map. ");
        return positionX <= 0;
    }

    private bool TileIsAtRightEdge(TransportNetworkTile tile)
    {
        int positionX = tile.position.x;
        if (positionX > tileMapWidth - 1)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.x = {positionX}, " +
                $"but the tile map only has a width of {tileMapWidth}. ");
        return positionX >= tileMapWidth - 1;
    }

}
