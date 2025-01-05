using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RecursiveBacktrackingMaze : MonoBehaviour
{
    public Tilemap tilemap; // The tilemap used for the maze
    public Tile pathTile;   // The tile used for paths
    public Tile wallTile;   // The tile used for walls
    
    // Maze size
    public int width;
    public int height;

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

    void GenerateMaze()
    {
        grid = new int[height, width];
        CarvePassagesFrom(0, 0);

        DrawMaze();
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
