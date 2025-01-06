using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Ability to generate the maze and keep it during edit mode,
// Provides the ability to make tweaks to the maze in case the generation went wrong. etc.
[ExecuteAlways]
public class RecursiveBacktrackingMaze : MonoBehaviour
{
    [Header("Maze Settings")]
    public int width = 10;
    public int height = 10;
    public int seed = -1; // Optional to randomise the maze or set a specific seed

    [Header("Tilemap and Tiles")]
    public Tilemap tilemap;
    public Tile pathTile;
    public Tile wallTile;

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

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        ClearMaze(); // Reset map

        // Set the random seed if a valid seed is provided
        if (seed >= 1)
        {
            UnityEngine.Random.InitState(seed);
        }
        else
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
            Debug.Log($"Generated Seed: {seed}");
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
        }
    }

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

    void DrawMaze()
    {
        // Make sure that the path tiles do not have a collider
        pathTile.colliderType = Tile.ColliderType.None;

        // Draw walls for the entire grid
        for (int y = 0; y < height * 2 + 1; y++)
        {
            for (int x = 0; x < width * 2 + 1; x++)
            {
                var position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, wallTile);
            }
        }

        // Draw paths based on the grid
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cell = grid[y, x];
                var basePosition = new Vector3Int(x * 2 + 1, y * 2 + 1, 0);

                // Draw the cell itself
                tilemap.SetTile(basePosition, pathTile);

                // Draw connections
                if ((cell & E) != 0)
                {
                    tilemap.SetTile(basePosition + Vector3Int.right, pathTile);
                }
                if ((cell & S) != 0)
                {
                    tilemap.SetTile(basePosition + Vector3Int.down, pathTile);
                }
            }
        }
    }

    void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
