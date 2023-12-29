using System.Collections.Generic;
using UnityEngine;

public enum BlockKind
{
    None,
    BlueBlock,
    GreenBlock,
    PurpleBlock,
    RedBlock,
    YellowBlock,
    DebugBlock
}

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
    [SerializeField] private GameObject debugBlock;

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

    void Start()
    {
        InitBoard();
        GenerateBlocks();

        Invoke("Test", 3f);
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

        InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 0 * (blockSize[0] + interval[0])), new Pos(0, 0));
        InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 1 * (blockSize[0] + interval[0])), new Pos(1, 0));
        InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 0 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 0));
        InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 1 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 1));
        InstantiateBlock(BlockKind.DebugBlock, spawnPosBase + (Vector3.right * 2 * (blockSize[1] + interval[1])) + (Vector3.down * 2 * (blockSize[0] + interval[0])), new Pos(2, 2));

        board[0][0] = BlockKind.DebugBlock;
        board[1][0] = BlockKind.DebugBlock;
        board[2][0] = BlockKind.DebugBlock;
        board[2][1] = BlockKind.DebugBlock;
        board[2][2] = BlockKind.DebugBlock;
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
        instanceBoard[posIndex.y][posIndex.x] = instance;
    }

    private void Test()
    {
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                CheckThreeMatch(new Pos(i, j));
            }
        }
    }

    // bfs로 구현
    private void CheckThreeMatch(Pos pos)
    {
        CheckVertical(pos);
        CheckHorizontal(pos);
    }

    private void CheckVertical(Pos pos)
    {
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        BlockKind blockKind = board[pos.y][pos.x];
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // 보드 범위 벗어나면 return
            if (currentPos.y < 0 || currentPos.y >= blockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= blockHorizontalSize)
                continue;

            // 이미 5개 매치했다면 탐색 종료
            if (matchData.Count == 5)
                break;

            // 1. 방문 여부 확인
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // 블럭 색이 다르면 다음으로
            if (board[currentPos.y][currentPos.x] != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. 상하 확인
            queue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
            queue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
        }

        // 3개 이상 매치했으면 가로로 2개 더 매치할 수 있는지 확인 필요
        // 요것도 아마 큐로 넣어뒀다가 하나씩 빼면서 확인하면 되지 않을까 싶음
        // 근데 4개, 5개일때는 찾을 필요 없음
        if (matchData.Count == 3)
        {
            for (int i = 0; i < matchData.Count; i++)
            {
                List<Pos> newMatchData = new List<Pos>();
                Queue<Pos> additionalQueue = new Queue<Pos>();
                additionalQueue.Enqueue(matchData[i]);
                isCheck[matchData[i].y][matchData[i].x] = false;
                while (additionalQueue.Count > 0)
                {
                    Pos currentPos = additionalQueue.Dequeue();

                    // 보드 범위 벗어나면 return
                    if (currentPos.y < 0 || currentPos.y >= blockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= blockHorizontalSize)
                        continue;

                    // 3개 매치했다면 탐색 종료
                    if (newMatchData.Count == 3)
                        break;

                    // 1. 방문 여부 확인
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // 블럭 색이 다르면 다음으로
                    if (board[currentPos.y][currentPos.x] != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. 좌우 확인
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
                }

                // 4개면은 추가 매치 성공으로 인정하지 않고 해당 부분은 미탐색 처리해주기
                if (newMatchData.Count < 3)
                {
                    // check 여부 초기화
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3개가 추가 매칭됐으면 더 보지말고 matchData에 추가 후 삭제하러 가기
                if (newMatchData.Count == 3)
                {
                    foreach (Pos newP in newMatchData)
                    {
                        if (matchData.Contains(newP) == false)
                            matchData.Add(newP);
                    }

                    break;
                }
            }
        }

        CheckBreak(matchData);
        matchData.Remove(pos);
        queue.Clear();
    }

    private void CheckHorizontal(Pos pos)
    {
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        isCheck[pos.y][pos.x] = false;
        BlockKind blockKind = board[pos.y][pos.x];
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // 보드 범위 벗어나면 return
            if (currentPos.y < 0 || currentPos.y >= blockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= blockHorizontalSize)
                continue;

            // 이미 5개 매치했다면 탐색 종료
            if (matchData.Count == 5)
                break;

            // 1. 방문 여부 확인
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // 블럭 색이 다르면 다음으로
            if (board[currentPos.y][currentPos.x] != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. 좌우 확인
            queue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
            queue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
        }

        // 3개 이상 매치했으면 가로로 2개 더 매치할 수 있는지 확인 필요
        // 요것도 아마 큐로 넣어뒀다가 하나씩 빼면서 확인하면 되지 않을까 싶음
        // 근데 4개, 5개일때는 찾을 필요 없음
        if (matchData.Count == 3)
        {
            for (int i = 0; i < matchData.Count; i++)
            {
                List<Pos> newMatchData = new List<Pos>();
                Queue<Pos> additionalQueue = new Queue<Pos>();
                additionalQueue.Enqueue(matchData[i]);
                isCheck[matchData[i].y][matchData[i].x] = false;
                while (additionalQueue.Count > 0)
                {
                    Pos currentPos = additionalQueue.Dequeue();

                    // 보드 범위 벗어나면 return
                    if (currentPos.y < 0 || currentPos.y >= blockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= blockHorizontalSize)
                        continue;

                    // 3개 매치했다면 탐색 종료
                    if (newMatchData.Count == 3)
                        break;

                    // 1. 방문 여부 확인
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // 블럭 색이 다르면 다음으로
                    if (board[currentPos.y][currentPos.x] != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. 상하 확인
                    additionalQueue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
                    additionalQueue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
                }

                // 4개면은 추가 매치 성공으로 인정하지 않고 해당 부분은 미탐색 처리해주기
                if (newMatchData.Count < 3)
                {
                    // check 여부 초기화
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3개가 추가 매칭됐으면 더 보지말고 matchData에 추가 후 삭제하러 가기
                if (newMatchData.Count == 3)
                {
                    foreach (Pos newP in newMatchData)
                    {
                        if (matchData.Contains(newP) == false)
                            matchData.Add(newP);
                    }

                    break;
                }
            }
        }

        CheckBreak(matchData);
        matchData.Remove(pos);
        queue.Clear();
    }

    //매치된 타일이 3개 이상이면 삭제
    private void CheckBreak(List<Pos> matchData)
    {
        if (matchData.Count < 3)
        {
            // check 여부 초기화
            foreach (Pos p in matchData)
                isCheck[p.y][p.x] = false;

            return;
        }

        Debug.Log(matchData.Count + " 개 부수기!");

        foreach (Pos p in matchData)
        {
            Debug.Log(p.y + "," + p.x);
            DestroyImmediate(instanceBoard[p.y][p.x]);
            board[p.y][p.x] = BlockKind.None;
        }
    }
}
