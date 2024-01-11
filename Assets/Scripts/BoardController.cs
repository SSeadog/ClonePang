using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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

    private int blockHorizontalSize = 5;
    private int blockVerticalSize = 5;

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

        //StartCoroutine(PangManager.Instance.Test());
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

    // board�����͸� �����ϴ� �Լ�
    public void SwapData(Pos a, Pos b)
    {
        BlockKind tempBlockKind = board[a.y][a.x];
        board[a.y][a.x] = board[b.y][b.x];
        board[b.y][b.x] = tempBlockKind;
    }

    private void BreakBlock(Pos pos)
    {
        if (instanceBoard[pos.y][pos.x] != null)
            DestroyImmediate(instanceBoard[pos.y][pos.x]);

        board[pos.y][pos.x] = BlockKind.None;
    }

    // �ִϸ��̼� �� ����
    public IEnumerator CoSwapBlock(Pos aPos, Pos bPos, UnityAction onSwapDoneAction)
    {
        Vector3 bBasePos = instanceBoard[bPos.y][bPos.x].transform.position;
        float animTime = 0.5f;
        while (animTime > 0f)
        {
            Vector3 aMoveVec = bBasePos - instanceBoard[aPos.y][aPos.x].transform.position;
            Vector3 bMoveVec = aMoveVec * -1;

            instanceBoard[aPos.y][aPos.x].transform.Translate(aMoveVec * Time.deltaTime * 10f);
            instanceBoard[bPos.y][bPos.x].transform.Translate(bMoveVec * Time.deltaTime * 10f);

            animTime -= Time.deltaTime;
            yield return null;
        }


        SwapBlock(aPos, bPos);
        onSwapDoneAction.Invoke();
    }

    // �ִϸ��̼� �� ����
    public IEnumerator CoBreak(Pos basePos, Pos breakPos)
    {
        PangManager.Instance.BreakAnimationCoroutineCount++;
        board[breakPos.y][breakPos.x] = BlockKind.None;

        float animTime = 0.5f;
        while (animTime > 0f)
        {
            if (GetInstanceBlock(breakPos) == null || GetInstanceBlock(basePos) == null)
                break;

            Vector3 moveVec = instanceBoard[basePos.y][basePos.x].transform.position - instanceBoard[breakPos.y][breakPos.x].transform.position;
            instanceBoard[breakPos.y][breakPos.x].transform.Translate(moveVec * Time.deltaTime * 10f);

            animTime -= Time.deltaTime;
            yield return null;
        }

        BreakBlock(breakPos);
        PangManager.Instance.BreakAnimationCoroutineCount--;
    }

    // Todo ����� ĭ ���� ������ų �� �Ʒ����� ������Ű�鼭
    // 1. ��ĭ ���������ϴ���
    // 2. �̹� ������ �� ����� �����
    // �� 2������ ���� ��ġ �����ϱ�
    // fallCount�� �Լ� ������ ĳ���ص� �� �Ʒ����� �� ���� �� ���� ��
    public void Refill()
    {
        // �� �Ʒ� + 1���� ��� ĭ�� Ž���ϸ鼭 �ڱ� �Ʒ��� �������� ��ĭ�� ����� ���� �����ֱ�
        for (int i = blockVerticalSize - 2; i >= 0; i--)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                Pos basePos = new Pos(i, j);
                
                // �ڱⰡ ��ĭ�̸� ���� ĭ �˻�
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
                    MoveBlock(basePos, new Pos(basePos.y + fallCount, basePos.x)); // ��ĭ ������ ���� �ʿ�
            }
        }

        // ��� ĭ�� �������� ����� ĭ ���� ���� ���������ֱ�
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                if (board[i][j] == BlockKind.None)
                {
                    Pos spawnPos = new Pos(i, j);
                    int rand = Random.Range(1, 6);
                    SpawnBlock(spawnPos, (BlockKind)rand); // ��ĭ ���������� Ȯ�� �ʿ�
                    
                }
            }
        }
    }

    public void ResetBoard()
    {
        GenerateBlocks();
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

    private GameObject GetInstanceBlock(Pos p)
    {
        return instanceBoard[p.y][p.x];
    }

    private void GenerateBlocks()
    {
        for (int i = 0; i < blockVerticalSize; i++)
        {
            for (int j = 0; j < blockHorizontalSize; j++)
            {
                Pos spawnPos = new Pos(i, j);
                BlockKind blockKind = (BlockKind)Random.Range(1, 6);
                //SpawnBlock(spawnPos, blockKind);
                Coroutine spawnBlockCoroutine = StartCoroutine(CoSpawnBlock(spawnPos, blockKind));
                while (PangManager.Instance.IsPang(spawnPos) == true)
                {
                    blockKind = (BlockKind)Random.Range(1, 6);
                    StopCoroutine(spawnBlockCoroutine);
                    spawnBlockCoroutine = StartCoroutine(CoSpawnBlock(spawnPos, blockKind));
                }
            }
        }

        if (PangManager.Instance.CheckSwapPang() == true)
        {
            Debug.Log("�����ؼ� ���� ��� ����");
        }
        else
        {
            Debug.Log("!! ��� �����ص� �� �ȵ� !!");
        }
    }

    private IEnumerator CoSpawnBlock(Pos pos, BlockKind kind)
    {
        if (GetBlock(pos) != BlockKind.None)
            BreakBlock(pos);

        board[pos.y][pos.x] = kind;

        // ������ ĭ �� �ޱ�? �ϴ� ������ 6ĭ ���� ����
        // ������ �ӵ��� �������� ��
        float fallSpeed = 400f;
        float fallDist = (-6f * (blockSize[0] + interval[0]));
        float finalYPos = GetSpawnCoord(pos).y;
        Vector3 spawnCoord = GetSpawnCoord(pos);
        GameObject blockInstance = InstantiateBlock(kind, spawnCoord, pos);

        Pos initPos = new Pos(pos.y - 6, pos.x);
        Vector3 initCoord = GetSpawnCoord(initPos);
        blockInstance.transform.localPosition = initCoord;

        float fallTime = Mathf.Abs(fallDist / fallSpeed);
        while (fallTime > 0f)
        {
            blockInstance.transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            fallTime -= Time.deltaTime;
            yield return null;
        }

        instanceBoard[pos.y][pos.x] = blockInstance;
    }

    private void SpawnBlock(Pos pos, BlockKind kind)
    {
        if (GetBlock(pos) != BlockKind.None)
            BreakBlock(pos);

        Vector3 spawnCoord = GetSpawnCoord(pos);
        GameObject blockInstance = InstantiateBlock(kind, spawnCoord, pos);
        board[pos.y][pos.x] = kind;
        instanceBoard[pos.y][pos.x] = blockInstance;
    }

    private GameObject InstantiateBlock(BlockKind block, Vector3 spawnPos, Pos posIndex)
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
            case BlockKind.DebugBlock:
                blockOrigin = debugBlock;
                break;
        }

        GameObject instance = Instantiate(blockOrigin, blockParent);
        instance.transform.localPosition = spawnPos;
        instance.GetComponent<Block>().Init(posIndex);

        return instance;
    }

    private void MoveBlock(Pos from, Pos to)
    {
        if (GetBlock(from) == BlockKind.None)
        {
            Debug.Log("�տ��� üũ�ߴµ� �� None����?");
            return;
        }
        board[to.y][to.x] = board[from.y][from.x];
        instanceBoard[to.y][to.x] = instanceBoard[from.y][from.x];
        if (instanceBoard[to.y][to.x] == null)
        {
            Debug.Log("�ٲ� �༮�� null�̴�!");
        }
        instanceBoard[to.y][to.x].transform.localPosition = GetSpawnCoord(to);
        instanceBoard[to.y][to.x].GetComponent<Block>().Init(to);

        board[from.y][from.x] = BlockKind.None;
        instanceBoard[from.y][from.x] = null;
    }

    // Util�� �ű�� �� ���� ��
    public Vector3 GetSpawnCoord(Pos p)
    {
        return spawnPosBase + (Vector3.right * p.x * (blockSize[1] + interval[1])) + (Vector3.down * p.y * (blockSize[0] + interval[0]));
    }
}
