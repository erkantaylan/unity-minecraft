using System.Collections.Generic;
using Models;
using UnityEngine;

public class World : MonoBehaviour
{
    private readonly List<ChunkCoordinate> activeChunkCoordinates = new List<ChunkCoordinate>();
    private Chunk[,] chunks;

    private ChunkCoordinate playerLastChunkCoord;
    
    private void Start()
    {
        SetSpawnPoint();
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordinates(player.position);
    }

    private void Update()
    {
        ChunkCoordinate lastChunkCoord = GetChunkCoordinates(player.position);
        if (lastChunkCoord.Equals(playerLastChunkCoord))
        {
            return;
        }
        
        playerLastChunkCoord = lastChunkCoord;
        
        CheckViewDistance();
    }

    private void SetSpawnPoint()
    {
        const float x = VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2f;
        const float z = VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2f;
        const int y = VoxelData.ChunkHeight + 2;
        spawnPosition = new Vector3(x, y, z);
    }

    private void GenerateWorld()
    {
        chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

        const int from = VoxelData.WorldSizeInChunks / 2 - VoxelData.ViewDistanceInChunks;
        const int to = VoxelData.WorldSizeInChunks / 2 + VoxelData.ViewDistanceInChunks;

        for (int x = from; x < to; x++)
        {
            for (int z = from; z < to; z++)
            {
                CreateNewChunk(x, z);
            }
        }

        player.position = spawnPosition;
    }

    //create chunk if it in players view distance
    private void CheckViewDistance()
    {
        ChunkCoordinate coordinate = GetChunkCoordinates(player.position);
        var previouslyActiavtedChunks = new List<ChunkCoordinate>(activeChunkCoordinates);
        int from = coordinate.X - VoxelData.ViewDistanceInChunks;
        int to = coordinate.X + VoxelData.ViewDistanceInChunks;
        for (int x = from; x < to; x++)
        {
            for (int z = from; z < to; z++)
            {
                var chunkCoordinate = new ChunkCoordinate(x, z);
                if (IsChunkInWorld(chunkCoordinate))
                {
                    Chunk chunk = chunks[x, z];
                    if (chunk == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if (!chunk.IsActive)
                    {
                        chunk.IsActive = true;
                        activeChunkCoordinates.Add(chunkCoordinate);
                    }
                }

                //remove if not in view distance???
                previouslyActiavtedChunks.RemoveAll(coord => coord.Equals(chunkCoordinate));

            }
        }

        foreach (ChunkCoordinate chunk in previouslyActiavtedChunks)
        {
            chunks[chunk.X, chunk.Z].IsActive = false;
        }
    }

    private static ChunkCoordinate GetChunkCoordinates(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);
        return new ChunkCoordinate(x, z);
    }

    public static byte GetVoxelId(Vector3Int position)
    {
        if (!IsVoxelInWorld(position))
        {
            return 0;
        }

        if (position.y == 0)
        {
            return 1;
        }

        if (position.y == VoxelData.ChunkHeight - 1)
        {
            return 2;
        }

        return 3;
    }

    private static bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < VoxelData.WorldSizeInVoxels
         && position.y >= 0 && position.y < VoxelData.WorldSizeInVoxels
         && position.z >= 0 && position.z < VoxelData.WorldSizeInVoxels;
    }

    private void CreateNewChunk(int x, int z)
    {
        var chunkCoordinate = new ChunkCoordinate(x, z);
        chunks[x, z] = new Chunk(this, chunkCoordinate);
        activeChunkCoordinates.Add(chunkCoordinate);
    }

    private static bool IsChunkInWorld(ChunkCoordinate coordinate)
    {
        return coordinate.X > 0 && coordinate.X < VoxelData.WorldSizeInChunks - 1
         && coordinate.Z > 0 && coordinate.Z < VoxelData.WorldSizeInChunks - 1;
    }

    #region Publics

    public BlockType[] blockTypes;


    public Material material;

    public Transform player;

    public Vector3 spawnPosition;

    #endregion
}