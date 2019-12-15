﻿using System.Collections;
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
    

    private bool tileIsAtUpperEdge(TransportNetworkTile tile)
    {
        int positionY = tile.position.y; 
        if (positionY > tileMapHeight - 1)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.y = {positionY}, " +
                $"but the tile map only has a height of {tileMapHeight}. "); 
        return positionY >= tileMapHeight - 1;
    }

    private bool tileIsAtLowerEdge(TransportNetworkTile tile)
    {
        int positionY = tile.position.y;
        if (positionY < 0)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.y = {positionY}, " +
                $"that is outside of the tile map. ");
        return positionY <= 0;
    }

    private bool tileIsAtLeftEdge(TransportNetworkTile tile)
    {
        int positionX = tile.position.x;
        if (positionX < 0)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.x = {positionX}, " +
                $"that is outside of the tile map. ");
        return positionX <= 0;
    }

    private bool tileIsAtRightEdge(TransportNetworkTile tile)
    {
        int positionX = tile.position.x;
        if (positionX > tileMapWidth - 1)
            Debug.LogError($"ERROR TRANSPORT NETWORK: Tile {tile} is at position.x = {positionX}, " +
                $"but the tile map only has a width of {tileMapWidth}. ");
        return positionX >= tileMapWidth - 1;
    }

}
