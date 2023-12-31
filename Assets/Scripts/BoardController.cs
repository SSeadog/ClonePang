using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class BoardController : MonoBehaviour
{
    [SerializeField] private Transform blockParent;
    [SerializeField] private GameObject blueBlock;
    [SerializeField] private GameObject greenBlock;
    [SerializeField] private GameObject purpleBlock;
    [SerializeField] private GameObject redBlock;
    [SerializeField] private GameObject yellowBlock;
    [SerializeField] private GameObject debugBlock;

    private int blockHorizontalSize = 7;
    private int blockVerticalSize = 6;

    private Vector2 boardVerticalPos = new Vector2(-300f, 300f);
    private Vector2 boardHorizontalPos = new Vector2(-360f, 360f);
    private Vector2 blockSize = new Vector2(100f, 100f);
    private Vector2 interval = new Vector2(20f, 20f);

    private Vector3 spawnPosBase = new Vector3(-360f, 300f, 100f);

    private BlockKind[][] board;
    private GameObject[][] instanceBoard;

    public int BlockHorizontalSize { get { return blockHorizontalSize; } }
    public int BlockVerticalSize { get { return blockVerticalSize; } }

    void Start()
    {
        InitBoard();
        GenerateBlocks();

        // 초기 세팅되면 매치되는 게 있는 게 확인해야함
        // 아니면 세팅할 때 아예 없게 세팅을 해야함
        StartCoroutine(Test());
    }

    public BlockKind GetBlock(Pos pos)
    {
        return board[pos.y][pos.x];
    }

    public void SwapBlock(Pos a, Pos b)
    {
        BlockKind tempBlockKind = board[a.y][a.x];
        board[a.y][a.x] = board[b.y][b.x];
        board[b.y][b.x] = tempBlockKind;

        GameObject tempInstance = instanceBoard[a.y][a.x];
        instanceBoard[a.y][a.x] = instanceBoard[b.y][b.x];
        instanceBoard[b.y][b.x] = tempInstance;

        instanceBoard[a.y][a.x].transform.localPosition = GetSpawnCoord(a);
        instanceBoard[b.y][b.x].transform.localPosition = GetSpawnCoord(b);

        instanceBoard[a.y][a.x].GetComponent<Block>().Init(a);
        instanceBoard[b.y][b.x].GetComponent<Block>().Init(b);
    }

    public void BreakBlock(Pos pos)
    {
        DestroyImmediate(instanceBoard[pos.y][pos.x]);
        board[pos.y][pos.x] = BlockKind.None;
    }

    public void Refill()
    {
        // 맨 아래 + 1부터 모든 칸을 탐색하면서 자기 아래를 기준으로 몇칸이 비는지 보고 내려주기
        for (int i = blockVerticalSize - 2; i >= 0; i--)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                Pos basePos = new Pos(i, j);
                // 자기가 빈칸이면 다음 칸 검사
                if (GetBlock(basePos) == BlockKind.None)
                    continue;

                int fallCount = 0;
                Pos checkPos = new Pos(i + 1, j);
                while (checkPos.y < blockVerticalSize && GetBlock(checkPos) == BlockKind.None)
                {
                    fallCount++;
                    checkPos = new Pos(checkPos.y + 1, checkPos.x);
                }

                if (fallCount > 0)
                    MoveBlock(basePos, new Pos(basePos.y + fallCount, basePos.x));
            }
        }

        // 모든 칸을 내렸으면 비워진 칸 보고 새로 생성시켜주기
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                if (board[i][j] == BlockKind.None)
                {
                    Pos spawnPos = new Pos(i, j);
                    Vector3 spawnCoord = GetSpawnCoord(spawnPos);
                    int rand = Random.Range(1, 6);
                    InstantiateBlock((BlockKind)rand, spawnCoord, spawnPos);
                    board[i][j] = (BlockKind)rand;
                }
            }
        }
    }

    private void InitBoard()
    {
        board = new BlockKind[blockVerticalSize][];
        instanceBoard = new GameObject[blockVerticalSize][];
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = new BlockKind[blockHorizontalSize];
            instanceBoard[i] = new GameObject[blockHorizontalSize];
        }
    }

    private void GenerateBlocks()
    {
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                Pos spawnPos = new Pos(i, j);
                Vector3 spawnCoord = GetSpawnCoord(spawnPos);
                int rand = Random.Range(1, 6);
                InstantiateBlock((BlockKind)rand, spawnCoord, spawnPos);
                board[i][j] = (BlockKind)rand;
            }
        }

        //InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 0 * (blockSize[0] + interval[0])), new Pos(0, 0));
        //InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 1 * (blockSize[0] + interval[0])), new Pos(1, 0));
        //InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 0));
        //InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 1 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 1));
        //InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 2 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 2));

        //board[0][0] = BlockKind.DebugBlock;
        //board[1][0] = BlockKind.DebugBlock;
        //board[2][0] = BlockKind.DebugBlock;
        //board[2][1] = BlockKind.DebugBlock;
        //board[2][2] = BlockKind.DebugBlock;
    }

    private void InstantiateBlock(BlockKind block, Vector3 spawnPos, Pos posIndex)
    {
        if (instanceBoard[posIndex.y][posIndex.x] != null)
        {
            DestroyImmediate(instanceBoard[posIndex.y][posIndex.x]);
        }

        GameObject blockOrigin = null;
        switch (block)
        {
            case BlockKind.BlueBlock:
                blockOrigin = blueBlock;
                break;
            case BlockKind.GreenBlock:
                blockOrigin = greenBlock;
                break;
            case BlockKind.PurpleBlock:
                blockOrigin = purpleBlock;
                break;
            case BlockKind.RedBlock:
                blockOrigin = redBlock;
                break;
            case BlockKind.YellowBlock:
                blockOrigin = yellowBlock;
                break;
            case BlockKind.DebugBlock:
                blockOrigin = debugBlock;
                break;
        }

        GameObject instance = Instantiate(blockOrigin, blockParent);
        instance.transform.localPosition = spawnPos;
        instance.GetComponent<Block>().Init(posIndex);
        instanceBoard[posIndex.y][posIndex.x] = instance;
    }

    private void MoveBlock(Pos from, Pos to)
    {
        board[to.y][to.x] = board[from.y][from.x];
        instanceBoard[to.y][to.x] = instanceBoard[from.y][from.x];
        instanceBoard[to.y][to.x].transform.localPosition = GetSpawnCoord(to);
        instanceBoard[to.y][to.x].GetComponent<Block>().Init(to);

        board[from.y][from.x] = BlockKind.None;
        instanceBoard[from.y][from.x] = null;
    }

    private Vector3 GetSpawnCoord(Pos p)
    {
        return spawnPosBase + (Vector3.right * p.x * (blockSize[1] + interval[1])) + (Vector3.down * p.y * (blockSize[0] + interval[0]));
    }

    private IEnumerator Test()
    {
        yield return new WaitForSeconds(1f);
        PangManager.Instance.State = State.Checking;
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                PangManager.Instance.CheckPang(new Pos(i, j));
            }
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(PangManager.Instance.CoRefill());
        PangManager.Instance.State = State.Playing;
    }
}
