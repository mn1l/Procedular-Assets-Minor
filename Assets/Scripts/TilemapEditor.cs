using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapEditor : MonoBehaviour
{
    public Tilemap tilemap; // The tilemap that needs to be used

    public Tile tileToPlace; // The tile asset that will be placed
    
    void Start()
    {
        SetTileAtPosition(new Vector3Int(0, 0, 0));
        SetTileAtPosition(new Vector3Int(1, 1, 0));
        SetTileAtPosition(new Vector3Int(-1, -1, 0));
    }
    
    // Place the tile at location
    public void SetTileAtPosition(Vector3Int position)
    {
        if (tileToPlace != null && tilemap != null)
        {
            tilemap.SetTile(position, tileToPlace);
        }
        else
        {
            Debug.LogError("Tile or Tilemap hasn't been assigned yet!");
        }
    }
    
    // Remove the tile at the location
    public void ClearTileAtPosition(Vector3Int position)
    {
        if (tilemap != null)
        {
            tilemap.SetTile(position, null);
        }
    }
}
