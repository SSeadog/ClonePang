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
    private Dictionary<Pos, List<Pos>> matchData = new Dictionary<Pos, List<Pos>>(); // 각 매치마다 한번씩 쓰니까 함수 안으로 넣는 게 좋을 듯

    private Dictionary<EMatchType, List<Vector2>> matchTypeData = new Dictionary<EMatchType, List<Vector2>>();

    void Start()
    {
        InitMatchTypeData();

        InitBoard();
        GenerateBlocks();

        Invoke("Test", 3f);
    }

    private void InitMatchTypeData()
    {
        matchTypeData.Add(EMatchType.vertical, new List<Vector2> { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-2, 0)});
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

        CheckBreak(pos);
        matchData.Remove(pos);
    }

    // 만약 세로 매치가 되었다
    // 그럼 basePos기준 delta값은 {(-1,0), (0,0), (1,0)} 또는 {(0,0), (-1,0), (-2,0)} 또는 {(0,0), (1,0), (2,0)}
    // 근데 왼쪽 위부터 탐색을 시작하니까 y축 값이 감소하는 케이스 밖에 안나올 거 같음 -> {(0,0), (-1,0), (-2,0)}


    // 매치 타입이 세로 3개인지 가로 3개인지 파악한 뒤 삭제
    private void CheckBreak(Pos pos)
    {
        if (matchData[pos].Count != 3)
            return;

        List<Vector2> deltas = new List<Vector2>();

        foreach (Pos p in matchData[pos])
        {
            deltas.Add(new Vector2(pos.y - p.y, pos.x - p.x));
        }

        for (int i = 0; i < deltas.Count; i++)
        {
            if (matchTypeData[EMatchType.vertical][i] != deltas[i])
            {
                Debug.Log("vertical 매치 검증 실패!");
                return;
            }
        }

        foreach (Pos p in matchData[pos])
        {
            Debug.Log(p.y + "," + p.x);
            DestroyImmediate(instanceBoard[p.y][p.x]);
            board[p.y][p.x] = BlockKind.None;
        }
    }
}
