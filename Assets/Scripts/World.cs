﻿using System.Collections.Generic;
using Models;
using UnityEngine;

public class World : MonoBehaviour
{
    private readonly List<ChunkCoordinate> activeChunks = new List<ChunkCoordinate>();

    private readonly Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    public BlockType[] blocktypes;

    public Material material;

    public Transform player;
    private ChunkCoordinate playerLastChunkCoordinate;
    public Vector3 spawn;

    private void Start()
    {
        GenerateWorld();
        playerLastChunkCoordinate = GetChunkCoordFromVector3(player.transform.position);
    }

    private void Update()
    {
        if (!GetChunkCoordFromVector3(player.transform.position).Equals(playerLastChunkCoordinate))
        {
            CheckViewDistance();
        }
    }

    private ChunkCoordinate GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoordinate(x, z);
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
                CreateChunk(new ChunkCoordinate(x, z));
            }
        }

        spawn = new Vector3(VoxelData.WorldSizeInBlocks / 2, VoxelData.ChunkHeight + 2, VoxelData.WorldSizeInBlocks / 2);
        player.position = spawn;
    }

    private void CheckViewDistance()
    {
        Vector3 position = player.position;
        int chunkX = Mathf.FloorToInt(position.x / VoxelData.ChunkWidth);
        int chunkZ = Mathf.FloorToInt(position.z / VoxelData.ChunkWidth);

        var previouslyActiveChunks = new List<ChunkCoordinate>(activeChunks);

        int fromX = chunkX - VoxelData.ViewDistanceInChunks / 2;
        int toX = chunkX + VoxelData.ViewDistanceInChunks / 2;
        
        int fromZ = chunkZ - VoxelData.ViewDistanceInChunks / 2;
        int toZ = chunkZ + VoxelData.ViewDistanceInChunks / 2;
        
        for (int x = fromX; x < toX; x++)
        {
            for (int z = fromZ; z < toZ; z++)
            {
                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(x, z))
                {
                    var thisChunk = new ChunkCoordinate(x, z);

                    if (chunks[x, z] == null)
                    {
                        CreateChunk(thisChunk);
                    }
                    else if (!chunks[x, z].IsActive)
                    {
                        chunks[x, z].IsActive = true;
                        activeChunks.Add(thisChunk);
                    }

                    // Check if this chunk was already in the active chunks list.
                    for (var i = 0; i < previouslyActiveChunks.Count; i++)
                    {
                        //if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        if (previouslyActiveChunks[i].X == x && previouslyActiveChunks[i].Z == z)
                        {
                            previouslyActiveChunks.RemoveAt(i);
                        }
                    }
                }
            }
        }

        foreach (ChunkCoordinate coord in previouslyActiveChunks)
        {
            chunks[coord.X, coord.Z].IsActive = false;
        }
    }

    private static bool IsChunkInWorld(int x, int z)
    {
        if (x > 0 && x < VoxelData.WorldSizeInChunks - 1 
         && z > 0 && z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }

        return false;
    }

    private void CreateChunk(ChunkCoordinate coordinate)
    {
        chunks[coordinate.X, coordinate.Z] = new Chunk(new ChunkCoordinate(coordinate.X, coordinate.Z), this);
        activeChunks.Add(new ChunkCoordinate(coordinate.X, coordinate.Z));
    }

    public static byte GetVoxelId(Vector3 pos)
    {
        if (pos.x < 0 || pos.x > VoxelData.WorldSizeInBlocks - 1 
         || pos.y < 0 || pos.y > VoxelData.ChunkHeight - 1 
         || pos.z < 0 || pos.z > VoxelData.WorldSizeInBlocks - 1)
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