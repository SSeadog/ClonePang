using System.Collections.Generic;
using UnityEngine;

public enum BlockKind
{
    None,
    BlueBlock,
    GreenBlock,
    PurpleBlock,
    RedBlock,
    YellowBlock
}

public enum EMatchType
{
    vertical,
    horizontal
};

public class Pos
{
    public int y;
    public int x;

    public Pos() { y = 0; x = 0; }
    public Pos(int y, int x) { this.y = y; this.x = x; }
}

public class PangManager_Copy : MonoBehaviour
{
    [SerializeField] private Transform blockParent;
    [SerializeField] private GameObject blueBlock;
    [SerializeField] private GameObject greenBlock;
    [SerializeField] private GameObject purpleBlock;
    [SerializeField] private GameObject redBlock;
    [SerializeField] private GameObject yellowBlock;

    private int blockHorizontalSize = 7;
    private int blockVerticalSize = 6;

    private Vector2 boardVerticalPos = new Vector2(-300f, 300f);
    private Vector2 boardHorizontalPos = new Vector2(-360f, 360f);
    private Vector2 blockSize = new Vector2(100f, 100f);
    private Vector2 interval = new Vector2(20f, 20f);

    private BlockKind[][] board;
    private GameObject[][] instanceBoard;

    private bool[][] isCheck;

    private Queue<Pos> queue = new Queue<Pos>();
    private Dictionary<Pos, List<Pos>> matchData = new Dictionary<Pos, List<Pos>>();

    void Start()
    {
        InitBoard();
        GenerateBlocks();
        CheckThreeMatch(new Pos(0, 0));
        //for (int i = 0; i < blockVerticalSize; i++)
        //{
        //    for (int j = 0; j < blockHorizontalSize; j++)
        //    {
        //        Pos pos = new Pos(i, j);
        //        //ThreeMatch(pos, board[i][j]);
        //        //ThreeMatch(pos, board[i][j]);
        //    }
        //}
    }

    private void InitBoard()
    {
        board = new BlockKind[blockVerticalSize][];
        instanceBoard = new GameObject[blockVerticalSize][];
        isCheck = new bool[blockVerticalSize][];
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = new BlockKind[blockHorizontalSize];
            instanceBoard[i] = new GameObject[blockHorizontalSize];
            isCheck[i] = new bool[blockHorizontalSize];
        }
    }

    private void GenerateBlocks()
    {
        Vector3 spawnPosBase = new Vector3(boardHorizontalPos[0], boardVerticalPos[1], 0f);

        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                Vector3 spawnPos = spawnPosBase + (Vector3.right * j * (blockSize[1] + interval[1])) + (Vector3.down * i * (blockSize[0] + interval[0]));
                int rand = Random.Range(1, 6);
                InstantiateBlock((BlockKind)rand, spawnPos, new Pos(i, j));
                board[i][j] = (BlockKind)rand;
            }
        }
    }

    private void InstantiateBlock(BlockKind block, Vector3 spawnPos, Pos posIndex)
    {
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
        }

        GameObject instance = Instantiate(blockOrigin, blockParent);
        instance.transform.localPosition = spawnPos;
        instanceBoard[posIndex.y][posIndex.x] = instance;
    }

    // bfs로 구현
    private void CheckThreeMatch(Pos pos)
    {
        queue.Enqueue(pos);
        BlockKind blockKind = board[pos.y][pos.x];
        matchData.Add(pos, new List<Pos>());
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // 보드 범위 벗어나면 return
            if (currentPos.y < 0 || currentPos.y >= blockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= blockHorizontalSize)
                continue;

            // 1. 방문 여부 확인
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // 블럭 색이 다르면 다음으로
            if (board[currentPos.y][currentPos.x] != blockKind)
                continue;

            matchData[pos].Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. 상하좌우 확인
            queue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
            queue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
            queue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
            queue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
        }

        foreach (Pos p in matchData[pos])
        {
            Debug.Log(p.y + "," + p.x);
        }

        //// matchData의 리스트가 1개 이하면 Dict에서 삭제
        //if (matchData[pos].Count <= 1)
        //    matchData.Remove(pos);
    }
}
