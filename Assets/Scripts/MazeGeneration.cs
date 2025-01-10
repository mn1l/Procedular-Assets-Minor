using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Ability to generate the maze and keep it during edit mode,
// Provides the ability to make tweaks to the maze in case the generation went wrong. etc.
[ExecuteAlways]
public class RecursiveBacktrackingMaze : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public int seed = -1; // Optional to randomise the maze or set a specific seed
    
    public Tilemap tilemap;
    public Tile pathTile;
    public Tile wallTile;
    public GameObject keyObject;
    public GameObject exitObject;

    private int[,] grid;    // Internal representation of the maze
    private const int N = 1, S = 2, E = 4, W = 8; // Directions

    private readonly Dictionary<int, Vector2Int> directionVectors = new Dictionary<int, Vector2Int>
    {
        { E, new Vector2Int(1, 0) },
        { W, new Vector2Int(-1, 0) },
        { N, new Vector2Int(0, -1) },
        { S, new Vector2Int(0, 1) }
    };
    private readonly Dictionary<int, int> opposites = new Dictionary<int, int>
    {
        { E, W },
        { W, E },
        { N, S },
        { S, N }
    };
    
    /*
     * 1. Clears the exisiting maze
     * 2. Sets the seed
     * 3. Initializes the internal grid representation of the maze
     * 4. Uses the recursive backtracking algorithm to carve the paths
     * 5. Visualizes the maze on the tilemap by calling DrawMaze
     */
    public void GenerateMaze()
    {
        ClearMaze();
        
        if (seed >= 1)
        {
            UnityEngine.Random.InitState(seed);
        }
        else
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }

        grid = new int[height, width];
        CarvePassagesFrom(0, 0);

        DrawMaze();
    }

    public void ClearMaze()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
            seed = -1;
        }
    }

    /*
     * Recursive method that creates the maze by carving passages between cells
     * Randomly shuffles directions and connects cells that haven't been visited yet
     */
    void CarvePassagesFrom(int cx, int cy)
    {
        var directions = new List<int> { N, S, E, W };
        Shuffle(directions);

        foreach (var direction in directions)
        {
            int nx = cx + directionVectors[direction].x;
            int ny = cy + directionVectors[direction].y;

            if (ny >= 0 && ny < height && nx >= 0 && nx < width && grid[ny, nx] == 0)
            {
                grid[cy, cx] |= direction;
                grid[ny, nx] |= opposites[direction];
                CarvePassagesFrom(nx, ny);
            }
        }
    }

    
    /*
     * Draws the entire maze on the Tilemap:
     * 1. Makes sure that the path tiles do not have a 2d collider
     * 2. Fills the entire area with walls
     * 3. Sets specific tiles as paths based on the internal grid representation
     * 4. Ensures connections between cells are visualized correctly
     */
    void DrawMaze()
    {
        pathTile.colliderType = Tile.ColliderType.None;

        // Fill the entire area with walls
        for (int y = 0; y < height * 2 + 1; y++)
        {
            for (int x = 0; x < width * 2 + 1; x++)
            {
                var position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, wallTile); // Default to walls
            }
        }

        // List to store all available path tiles (empty spaces in the maze)
        List<Vector3Int> pathTiles = new List<Vector3Int>();

        // Draw the maze based on the grid representation
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cell = grid[y, x];
                var basePosition = new Vector3Int(x * 2 + 1, y * 2 + 1, 0);

                // Draw the cell itself as a path
                tilemap.SetTile(basePosition, pathTile);

                // Add the path tile to the list
                pathTiles.Add(basePosition);

                // Draw connections based on the carved paths in the grid
                if ((cell & E) != 0) // East connection
                {
                    tilemap.SetTile(basePosition + Vector3Int.right, pathTile);
                }
                if ((cell & S) != 0) // South connection
                {
                    tilemap.SetTile(basePosition + Vector3Int.down, pathTile);
                }
            }
        }

        // Add the entrance (top-left corner) as a path tile
        var entrancePosition = new Vector3Int(1, height * 2, 0); // Top-left
        tilemap.SetTile(entrancePosition, pathTile);

        // Add the exit (bottom-right corner) as a GameObject
        var exitPosition = new Vector3Int(width * 2 - 1, 0, 0); // Bottom-right
        tilemap.SetTile(exitPosition, pathTile); // Still set a path tile for consistency

        // Find the existing exit GameObject and place it
        if (exitObject != null)
        {
            // Find the existing exit object by name (you can also use a tag if you prefer)
            GameObject existingExit = GameObject.Find("Exit");

            if (existingExit != null)
            {
                // Convert tilemap coordinates to world position
                Vector3 worldPosition = tilemap.CellToWorld(exitPosition);

                // Adjust the position to center the object
                worldPosition.x += tilemap.cellSize.x / 2f;
                worldPosition.y += tilemap.cellSize.y / 2f;

                // Move the existing exit object to the new position
                existingExit.transform.position = worldPosition;
            }
            else
            {
                Instantiate(exitObject.gameObject, tilemap.CellToWorld(exitPosition), Quaternion.identity);
            }
        }

        // Randomly place 3 Key GameObjects on path tiles
        if (keyObject != null && pathTiles.Count > 3)
        {
            for (int i = 0; i < 3; i++)
            {
                // Randomly pick a path tile from the list
                int randomIndex = UnityEngine.Random.Range(0, pathTiles.Count);
                Vector3Int randomTilePosition = pathTiles[randomIndex];

                // Convert tilemap coordinates to world position
                Vector3 worldPosition = tilemap.CellToWorld(randomTilePosition);

                // Adjust the position to center the object
                worldPosition.x += tilemap.cellSize.x / 2f;
                worldPosition.y += tilemap.cellSize.y / 2f;

                // Instantiate the Key GameObject
                GameObject keyInstance = Instantiate(keyObject.gameObject, worldPosition, Quaternion.identity);
                keyInstance.name = "Key_" + i;
                
                // Optionally remove the placed key from the list to avoid reusing the same tile
                pathTiles.RemoveAt(randomIndex);
            }
        }

        // Surround the maze with walls to ensure no other exits
        for (int y = 0; y < height * 2 + 1; y++)
        {
            for (int x = 0; x < width * 2 + 1; x++)
            {
                // Skip entrance and exit positions
                if ((y == height * 2 && x == 1) || (y == 0 && x == width * 2 - 1))
                {
                    continue;
                }

                // Ensure no gaps in the boundary
                if (x == 0 || x == width * 2 || y == 0 || y == height * 2)
                {
                    var position = new Vector3Int(x, y, 0);
                    tilemap.SetTile(position, wallTile);
                }
            }
        }
}




    
    /*
     * Randomly shuffle the elements of a list to randomize the direction order in CarvePassagesFrom
     */
    void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
