using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private Transform player;
    public PlayerManager pm;
    public GameObject[] tilePrefabs;
    public float zSpawn = 0;
    public float tileLength = 30;
    public int numberOfTiles = 5;
    private List<GameObject> activeTiles = new List<GameObject>();
    public List<GameObject> landscape;
    // Start is called before the first frame update

    public Transform playerTransform;
    void Start()
    {
        player = GameObject.Find("Player").transform;

        for (int i = 0; i < numberOfTiles; i++)
        {

            if (i == 0)
            {
                SpawnTile(0);
            }
            else
                SpawnTile(Random.Range(0, tilePrefabs.Length));
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the number of tiles to spawn ahead of the player
        int tilesToSpawnAhead = Mathf.FloorToInt((playerTransform.position.z - zSpawn) / tileLength);

        // Determine how many tiles need to be spawned
        int tilesToSpawn = numberOfTiles - activeTiles.Count;

        // Spawn the required number of tiles
        for (int i = 0; i < tilesToSpawn + 10; i++)
        {
            SpawnTile(Random.Range(0, tilePrefabs.Length));
        }

        // Remove excess tiles beyond the numberOfTiles
        // while (activeTiles.Count > numberOfTiles)
        // {
        //     DeleteTile();
        // }

        // Delete tiles that are behind the player
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            if (activeTiles[i].transform.position.z < playerTransform.position.z - tileLength)
            {
                DeleteTile(i);
            }
        }
    }




    public void SpawnTile(int tileIndex)
    {
        // Debug.Log("Spawning Tile at zSpawn: " + zSpawn);
        GameObject go = Instantiate(tilePrefabs[tileIndex], transform.forward * zSpawn, transform.rotation);
        activeTiles.Add(go);
        // Debug.Log("Added Tile in list: " + activeTiles[0].name);
        zSpawn += tileLength;
    }
    private void DeleteTile(int i = 0)
    {
        // Debug.Log("Deleting Tile");
        Destroy(activeTiles[i]);
        activeTiles.RemoveAt(i);
    }

    public void SpawnLandscape()
    {
        if (player != null)
        {
            print("lanscape spawned");
            GameObject movedLandscape = landscape[0];
            landscape.Remove(movedLandscape);
            float newz = landscape[0].transform.position.z + 200f;
            movedLandscape.transform.position = new Vector3(0f, 0f, newz);
            landscape.Add(movedLandscape);
        }
    }
}
