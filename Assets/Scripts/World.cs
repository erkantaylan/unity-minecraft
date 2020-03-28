using System;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    private readonly List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public BlockType[] blocktypes;

    private readonly Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    public Material material;

    public Transform player;
    private ChunkCoord playerLastChunkCoord;
    public Vector3 spawn;

    private void Start()
    {
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
    }

    private void Update()
    {
        if (!GetChunkCoordFromVector3(player.transform.position).Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
        }
    }

    private ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    private void GenerateWorld()
    {
        for (int x = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks / 2;
            x < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks / 2;
            x++)
        {
            for (int z = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks / 2;
                z < VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks / 2;
                z++)
            {
                CreateChunk(new ChunkCoord(x, z));
            }
        }

        spawn = new Vector3(VoxelData.WorldSizeInBlocks / 2, VoxelData.ChunkHeight + 2, VoxelData.WorldSizeInBlocks / 2);
        player.position = spawn;
    }

    private void CheckViewDistance()
    {
        int chunkX = Mathf.FloorToInt(player.position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(player.position.z / VoxelData.ChunkWidth);

        var previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {
                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(x, z))
                {
                    var thisChunk = new ChunkCoord(x, z);

                    if (chunks[x, z] == null)
                    {
                        CreateChunk(thisChunk);
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(thisChunk);
                    }

                    // Check if this chunk was already in the active chunks list.
                    for (var i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        //if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        foreach (ChunkCoord coord in previouslyActiveChunks)
        {
            chunks[coord.x, coord.z].isActive = false;
        }
    }

    private bool IsChunkInWorld(int x, int z)
    {
        if (x > 0 && x < VoxelData.WorldSizeInChunks - 1 && z > 0 && z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }

        return false;
    }

    private void CreateChunk(ChunkCoord coord)
    {
        chunks[coord.x, coord.z] = new Chunk(new ChunkCoord(coord.x, coord.z), this);
        activeChunks.Add(new ChunkCoord(coord.x, coord.z));
    }

    public byte GetVoxel(Vector3 pos)
    {
        if (pos.x < 0 || pos.x > VoxelData.WorldSizeInBlocks - 1 || pos.y < 0 || pos.y > VoxelData.ChunkHeight - 1 || pos.z < 0 ||
            pos.z > VoxelData.WorldSizeInBlocks - 1)
        {
            return 0;
        }

        if (pos.y < 1)
        {
            return 1;
        }

        if (pos.y == VoxelData.ChunkHeight - 1)
        {
            return 3;
        }

        return 2;
    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.x == x && other.z == z)
        {
            return true;
        }

        return false;
    }
}

[Serializable]
public class BlockType
{
    [Header("Texture Values")]
    public int backFaceTexture;

    public string blockName;
    public int bottomFaceTexture;
    public int frontFaceTexture;
    public bool isSolid;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public int topFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }
    }
}