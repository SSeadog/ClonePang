using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockKind
{
    None,
    Yellow
}

public class PangManager : MonoBehaviour
{
    [SerializeField] private Transform blockParent;
    [SerializeField] private GameObject yellowBlock;

    private int blockHorizontalSize = 7;
    private int blockVerticalSize = 6;

    private Vector2 boardVerticalPos = new Vector2(-300f, 300f);
    private Vector2 boardHorizontalPos = new Vector2(-360f, 360f);
    private Vector2 blockSize = new Vector2(100f, 100f);
    private Vector2 interval = new Vector2(20f, 20f);

    private BlockKind[][] board;

    void Start()
    {
        InitBoard();
        GenerateBlocks();
    }

    private void InitBoard()
    {
        board = new BlockKind[blockVerticalSize][];
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = new BlockKind[blockHorizontalSize];
        }
    }

    private void GenerateBlocks()
    {
        Vector3 spawnPosBase = new Vector3(boardHorizontalPos[0], boardVerticalPos[1], 0f);

        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                board[i][j] = BlockKind.Yellow;
                Vector3 spawnPos = spawnPosBase + (Vector3.right * j * (blockSize[1] + interval[1])) + (Vector3.down * i * (blockSize[0] + interval[0]));
                InstantiateBlock(spawnPos);
            }
        }
    }

    private void InstantiateBlock(Vector3 spawnPos)
    {
        GameObject instance = Instantiate(yellowBlock, blockParent);
        instance.transform.localPosition = spawnPos;
    }

    void Update()
    {
        
    }
}
