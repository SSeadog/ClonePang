//using System.Collections.Generic;
//using UnityEngine;

//public enum BlockKind
//{
//    None,
//    BlueBlock,
//    GreenBlock,
//    PurpleBlock,
//    RedBlock,
//    YellowBlock
//}

//public enum EMatchType
//{
//    vertical,
//    horizontal
//};

//public class Pos
//{
//    public int y;
//    public int x;

//    public Pos() { y = 0; x = 0; }
//    public Pos(int y, int x) { this.y = y; this.x = x; }
//}

//public class PangManager : MonoBehaviour
//{
//    [SerializeField] private Transform blockParent;
//    [SerializeField] private GameObject blueBlock;
//    [SerializeField] private GameObject greenBlock;
//    [SerializeField] private GameObject purpleBlock;
//    [SerializeField] private GameObject redBlock;
//    [SerializeField] private GameObject yellowBlock;

//    private int blockHorizontalSize = 7;
//    private int blockVerticalSize = 6;

//    private Vector2 boardVerticalPos = new Vector2(-300f, 300f);
//    private Vector2 boardHorizontalPos = new Vector2(-360f, 360f);
//    private Vector2 blockSize = new Vector2(100f, 100f);
//    private Vector2 interval = new Vector2(20f, 20f);

//    private BlockKind[][] board;
//    private GameObject[][] instanceBoard;

//    private bool[][] isCheck;

//    void Start()
//    {
//        InitBoard();
//        GenerateBlocks();
//        for (int i = 0; i < blockVerticalSize; i++)
//        {
//            for (int j = 0; j < blockHorizontalSize; j++)
//            {
//                Pos pos = new Pos(i, j);
//                ThreeMatch(pos, board[i][j], EMatchType.vertical);
//                ThreeMatch(pos, board[i][j], EMatchType.horizontal);
//            }
//        }
//    }

//    private void InitBoard()
//    {
//        board = new BlockKind[blockVerticalSize][];
//        instanceBoard = new GameObject[blockVerticalSize][];
//        isCheck = new bool[blockVerticalSize][];
//        for (int i = 0; i < board.Length; i++)
//        {
//            board[i] = new BlockKind[blockHorizontalSize];
//            instanceBoard[i] = new GameObject[blockHorizontalSize];
//            isCheck[i] = new bool[blockHorizontalSize];
//        }
//    }

//    private void GenerateBlocks()
//    {
//        Vector3 spawnPosBase = new Vector3(boardHorizontalPos[0], boardVerticalPos[1], 0f);

//        for (int i = 0; i < blockVerticalSize; i++)
//        {
//            for (int j = 0; j < blockHorizontalSize; j++)
//            {
//                board[i][j] = BlockKind.YellowBlock;
//                Vector3 spawnPos = spawnPosBase + (Vector3.right * j * (blockSize[1] + interval[1])) + (Vector3.down * i * (blockSize[0] + interval[0]));
//                int rand = Random.Range(1, 6);
//                InstantiateBlock((BlockKind)rand, spawnPos, new Pos(i, j));
//            }
//        }
//    }

//    private void InstantiateBlock(BlockKind block, Vector3 spawnPos, Pos posIndex)
//    {
//        GameObject blockOrigin = null;
//        switch (block)
//        {
//            case BlockKind.BlueBlock:
//                blockOrigin = blueBlock;
//                break;
//            case BlockKind.GreenBlock:
//                blockOrigin = greenBlock;
//                break;
//            case BlockKind.PurpleBlock:
//                blockOrigin = purpleBlock;
//                break;
//            case BlockKind.RedBlock:
//                blockOrigin = redBlock;
//                break;
//            case BlockKind.YellowBlock:
//                blockOrigin = yellowBlock;
//                break;
//        }

//        GameObject instance = Instantiate(blockOrigin, blockParent);
//        instance.transform.localPosition = spawnPos;
//        instanceBoard[posIndex.y][posIndex.x] = instance;
//    }

//    //private void ThreeMatch(Pos pos, BlockKind find, EMatchType matchType, List<Pos> matchList = null, int recursionLevel = 0)
//    //{
//    //    if (isCheck[pos.y][pos.x] == true)
//    //        return;

//    //    // 매치 되면 없애기
//    //    if (matchList != null && matchList.Contains(pos))
//    //        return;

//    //    if (pos.y < 0) return;
//    //    if (pos.y > blockVerticalSize - 1) return;
//    //    if (pos.x < 0) return;
//    //    if (pos.x > blockHorizontalSize - 1) return;

//    //    BlockKind boardData = board[pos.y][pos.x];

//    //    if (boardData == BlockKind.None)
//    //        return;

//    //    Debug.Log($"{pos.y},{pos.x}");

//    //    if (boardData == find)
//    //    {
//    //        if (matchList == null)
//    //            matchList = new List<Pos>();

//    //        matchList.Add(pos);
//    //        isCheck[pos.y][pos.x] = true;

//    //        if (matchType == EMatchType.vertical)
//    //        {
//    //            ThreeMatch(new Pos(pos.y + 1, pos.x), find, EMatchType.vertical, matchList, recursionLevel + 1);
//    //            ThreeMatch(new Pos(pos.y - 1, pos.x), find, EMatchType.vertical, matchList, recursionLevel + 1);
//    //        }
//    //        else if (matchType == EMatchType.horizontal)
//    //        {
//    //            ThreeMatch(new Pos(pos.y, pos.x + 1), find, EMatchType.horizontal, matchList, recursionLevel + 1);
//    //            ThreeMatch(new Pos(pos.y, pos.x - 1), find, EMatchType.horizontal, matchList, recursionLevel + 1);
//    //        }
//    //    }

//    //    if (recursionLevel == 0)
//    //    {
//    //        if (matchList.Count >= 3)
//    //        {
//    //            for (int i = 0; i < matchList.Count; i++)
//    //            {
//    //                int y = matchList[i].y;
//    //                int x = matchList[i].x;

//    //                Debug.Log($"{x},{y}"); // 한줄에 표시 필요
//    //                Destroy(instanceBoard[y][x]);
//    //                board[y][x] = BlockKind.None;
//    //                instanceBoard[y][x] = null;
//    //                matchList[i] = null;
//    //            }
//    //        }
//    //    }
//    //}
//}
