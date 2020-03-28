using Models;
using UnityEngine;

public class World : MonoBehaviour
{
    public BlockType[] blockTypes;

    private Chunk[,] chunks;

    public Material material;

    public Transform player;

    public Vector3 spawnPosition;

    private void Start()
    {
        float x = VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2f;
        float z = VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth / 2f;
        spawnPosition = new Vector3(x, VoxelData.ChunkHeight + 1, z);
        GenerateWorld();
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

    public byte GetVoxelId(Vector3Int position)
    {
        if (!IsVoxelInWorld(position))
        {
            return 3;
        }

        if (position.y < 1)
        {
            return 0;
        }

        if (position.y == VoxelData.ChunkHeight - 1)
        {
            return 2;
        }

        return 1;
    }

    private static bool IsVoxelInWorld(Vector3 position)
    {
        return position.x >= 0 && position.x < VoxelData.WorldSizeInVoxels
         && position.y >= 0 && position.y < VoxelData.WorldSizeInVoxels
         && position.z >= 0 && position.z < VoxelData.WorldSizeInVoxels;
    }

    private void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(this, new ChunkCoordinates(x, z));
    }

    private bool IsChunkInWorld(ChunkCoordinates coordinates)
    {
        return coordinates.X > 0 && coordinates.X < VoxelData.WorldSizeInChunks - 1
         && coordinates.Z > 0 && coordinates.Z < VoxelData.WorldSizeInChunks - 1;
    }
}