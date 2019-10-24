using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages what happens when a node is clicked
/// Tiles can only be placed if they are anchored
/// When a tile is removed and the remaining ones are no longer anchored
/// then they should all be removed
/// </summary>
public class TileNodeManager : Singleton<TileNodeManager>
{
    [SerializeField, Tooltip("x and y length of the grid")]
    Vector2 m_gridSize = new Vector2(7, 7);

    /// <summary>
    /// In the rush of things I neglected to make nodes, anchors, and placeable tiles
    /// be part of a base class that we could easily create a 2d grid where we can place
    /// them base on their x, y and use that when testing availability and support.
    /// 
    /// For now, I am using a second array that only contains the tiles and this is what 
    /// we will use to test for neighbors, tile support, and availability
    /// </summary>
    Tile[,] m_tiles;

    /// <summary>
    /// A grid of all the nodes on the board so that we can quickly
    /// enable/disable them as tiles are placed/removed over them
    /// </summary>
    TileNode[,] m_nodes;

    /// <summary>
    /// The grid contains all locations of nodes and anchor points
    /// Nodes are represented with NULLs while anchors have an GO
    /// </summary>
    AnchorPoint[,] m_anchors;

    /// <summary>
    /// Builds the node grid
    /// </summary>
    private void Start()
    {
        m_tiles = InitializeGrid(m_tiles);
        m_nodes = InitializeGrid(m_nodes);
        m_anchors = InitializeGrid(m_anchors);

        var anchor = GetObjectAtPosition(new Vector2(4, 0), m_anchors);
    }

    /// <summary>
    /// Initializes the given grid based on the overall grid size
    /// Finds and adds all objects of the given type based on their position
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    private T[,] InitializeGrid<T>(T[,] grid) where T : MonoBehaviour
    {
        grid = new T[(int)m_gridSize.x, (int)m_gridSize.y];
        foreach (var item in FindObjectsOfType<T>()) {
            var point = item.transform.position;

            if (IsInBounds(point, grid)) {
                var x = (int)point.x;
                var y = (int)point.y;
                grid[x, y] = item;
            }
        }

        return grid;
    }

    /// <summary>
    /// True when the given position exists within the given grid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private bool IsInBounds<T>(Vector2 position, T[,] grid) where T : MonoBehaviour
    {
        var xInBound = (int)position.x >= 0 && (int)position.x < grid.GetLength(0);
        var yInBound = (int)position.y >= 0 && (int)position.y < grid.GetLength(1);

        return xInBound && yInBound;
    }

    /// <summary>
    /// Returns the GameObject located on the grid at the given point
    /// if the point is valid
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private T GetObjectAtPosition<T>(Vector2 position, T[,] grid) where T : MonoBehaviour
    {
        T go = null;

        var x = (int)position.x;
        var y = (int)position.y;

        var xInBound = x >= 0 && x < grid.GetLength(0);
        var yInBound = y >= 0 && y < grid.GetLength(1);

        if (xInBound && yInBound) {
            go = grid[x, y];
        }

        return go;
    }

    /// <summary>
    /// Adds the given item to the given grid as long as it is within bounds
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="grid"></param>
    private void AddObjectToGrid<T>(T item, T[,] grid) where T : MonoBehaviour
    {
        var position = item.transform.position;
        if (IsInBounds(position, grid)) {
            grid[(int)position.x, (int)position.y] = item;
        }
    }

    /// <summary>
    /// Nullifies the position of the given item within the given grid if it is within bounds
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <param name="grid"></param>
    private void RemoveObjectAt<T>(T item, T[,] grid) where T : MonoBehaviour
    {
        var position = item.transform.position;
        if (IsInBounds(position, grid)) {
            grid[(int)position.x, (int)position.y] = null;
        }
    }

    /// <summary>
    /// True when at least one of its adjacent neighbors is not nulll
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="position"></param>
    /// <param name="grid"></param>
    /// <returns></returns>
    private bool IsAnchoredTo<T>(Vector2 position, T[,] grid) where T : MonoBehaviour
    {
        var isAnchored = false;

        foreach (var point in Utilities.CaridanalPoints) {
            var adjacent = position + point;
            var neighbor = GetObjectAtPosition(adjacent, grid);

            isAnchored = neighbor != null;
            if (isAnchored) {
                break;
            }
        }

        return isAnchored;
    }

    /// <summary>
    /// Spawns the currently selected tile on the node clicked
    /// </summary>
    /// <param name="node"></param>
    public void OnNodeClicked(TileNode node)
    {
        if (!NodeIsAvailableForTilePlacement(node)) {
            AudioManager.Instance.Play2DSound(AudioClipName.Unavailable);
            return;
        }

        var position = new Vector3(node.transform.position.x, node.transform.position.y, 0f);
        Tile prefab = InventoryManager.Instance.GetCurrentPrefabToSpawn();

        if (prefab) {
            // Disable node so that it cannot be accidentally clicked
            node.IsActive = false;

            var tile = Instantiate(prefab, position, Quaternion.identity, transform).GetComponent<Tile>();
            tile.name = $"{prefab.name}_{position.x}_{position.y}";

            AddObjectToGrid(tile, m_tiles);
            AudioManager.Instance.Play2DSound(AudioClipName.TilePlaced);
        }
    }

    /// <summary>
    /// True when the given node is at position where a tile would be anchored
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool NodeIsAvailableForTilePlacement(TileNode node)
    {
        var position = new Vector3(node.transform.position.x, node.transform.position.y, 0f);

        // This tile is already in use
        if (GetObjectAtPosition(position, m_tiles) != null) {
            return false;
        }

        // If tile is adjecent to an anchor point or another tile then we can add it
        bool isAnchored = IsAnchoredTo(position, m_anchors) || IsAnchoredTo(position, m_tiles);

        // Cannot place the tile
        if (!isAnchored) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Removes the tile from the grid tiles and re-enables the node underneath
    /// </summary>
    /// <param name="tile"></param>
    public void OnTileRemoved(Tile tile)
    {
        var position = tile.transform.position;       

        if(GetObjectAtPosition(position, m_tiles) != null) {
            AudioManager.Instance.Play2DSound(AudioClipName.TileRemoved);
            RemoveObjectAt(tile, m_tiles);
        }

        // We handle the re-activating of the node afterwards
        // as the tile may be gone by the time this code is hit
        // and we should make sure the node is always enable when there's not tile
        var node = GetObjectAtPosition(position, m_nodes);
        if (node != null)
            node.IsActive = true;

        ValidateTileSupport();
    }

    /// <summary>
    /// Loop through all remaining tiles grabbing their neighbors
    /// As long as at least ONE neighbor is anchored then they are all supported
    /// otherwise we must trigger them to be removed
    /// </summary>
    private void ValidateTileSupport()
    {
        // hack to remove duplicates that we still have
        var removedNames = new List<string>();

        foreach (var tile in FindObjectsOfType<Tile>()) {
            // Tile has been or is being remove therefore ignore it
            if(tile == null || tile.IsShrinking || removedNames.Contains(tile.name)) {
                continue;
            }

            var neighbors = GetAllNeighboringTiles(tile);
            neighbors.Add(tile); // adding itself to be tested to see if it anchor

            var nodes = new Queue<Tile>(neighbors);
            var isAnchored = false;

            do {
                var node = nodes.Dequeue();
                isAnchored = IsAnchoredTo(node.transform.position, m_anchors);

                // At least one is anchored
                if (isAnchored) {
                    isAnchored = true;
                    nodes.Clear();
                }

            } while (nodes.Count > 0);

            // Trigger all to be removed
            if (!isAnchored) {

                // Make sure we only have uniques
                neighbors = neighbors.Distinct().ToList();

                foreach (var neighbor in neighbors) {
                    if (removedNames.Contains(neighbor.name)) {
                        continue;
                    }

                    removedNames.Add(neighbor.name);
                    neighbor.TriggerRemove(false);
                }
            }
        }
    }

    /// <summary>
    /// Returns a collection of all the adjacent neighbors the given tile has
    /// This means that all the tiles in the given collection are connected on
    /// at least on their edges
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    private List<Tile> GetAllNeighboringTiles(Tile root)
    {
        var neighbors = new List<Tile>();
        var unvisited = new Queue<Tile>();
        var visited = new List<Tile>();
        unvisited.Enqueue(root);

        do {
            // Visiting the next unvisited 
            var parent = unvisited.Dequeue();
            visited.Add(parent);

            neighbors.AddRange(GetNeighborgTiles(parent));
            neighbors = neighbors.Distinct().ToList();

            // Grab all new unvisited tiles to enqueue them
            var tiles = neighbors.Except(visited).ToList();
            tiles.ForEach(n => unvisited.Enqueue(n));
        } while (unvisited.Count > 0);

        return neighbors;
    }

    /// <summary>
    /// Returns a list of adjacent neighbors
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="tileGrid"></param>
    /// <returns></returns>
    private List<Tile> GetNeighborgTiles(Tile parent)
    {
        var neighborgs = new List<Tile>();
        var position = new Vector2(parent.transform.position.x, parent.transform.position.y);

        foreach (var point in Utilities.CaridanalPoints) {
            var adjacent = position + point;
            var neighbor = GetObjectAtPosition(adjacent, m_tiles);            

            // Ignore tiles shrinking as those are being removed
            // therefore they would no longer be a neighbor
            if (neighbor != null && !neighbor.IsShrinking) {
                neighborgs.Add(neighbor);
            }
        }

        return neighborgs;
    }

    /// <summary>
    /// Returns the tile at the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Tile GetTileAt(Vector2 position)
    {
        return GetObjectAtPosition(position, m_tiles);
    }

    /// <summary>
    /// REturns the anchor point at the given position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public AnchorPoint GetAnchorPointAt(Vector2 position)
    {
        return GetObjectAtPosition(position, m_anchors);
    }
}
