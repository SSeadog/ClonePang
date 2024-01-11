using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public enum State
{
    Playing,
    Checking
}

public class PangManager : MonoBehaviour
{
    private static PangManager instance;
    public static PangManager Instance { get { return instance; } }

    [SerializeField] private SelectBorder firstSelectBorder;
    [SerializeField] private SelectBorder secondSelectBorder;

    [SerializeField] private GameObject touchBlock;

    private BoardController board;

    private bool[][] isCheck;
    private Queue<Pos> queue = new Queue<Pos>();

    private Pos firstSelect;
    private Pos secondSelect;

    private int breakAnimationCoroutineCount;

    public int BreakAnimationCoroutineCount { get { return breakAnimationCoroutineCount; } set { breakAnimationCoroutineCount = value; } }

    private State state;

    public State State {
        get
        {
            return state;
        }
        set
        {
            if (state == value)
                return;

            state = value;

            if (state == State.Playing)
                touchBlock.SetActive(false);
            else if (state == State.Checking)
                touchBlock.SetActive(true);
        }
    }

    private void Start()
    {
        instance = this;
        board = FindObjectOfType<BoardController>();

        isCheck = new bool[board.BlockVerticalSize][];
        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            isCheck[i] = new bool[board.BlockHorizontalSize];
        }
    }

    public IEnumerator Test()
    {
        yield return new WaitForSeconds(1f);
        State = State.Checking;
        //for (int i = 0; i < board.BlockVerticalSize; i++)
        //{
        //    for (int j = 0; j < board.BlockHorizontalSize; j++)
        //    {
        //        CheckPang(new Pos(i, j));
        //    }
        //}
        Pang(new Pos(2, 2));

        yield return new WaitForSeconds(1f);
        StartCoroutine(CoRefill());
    }

    public bool Pang(Pos pos)
    {
        bool verticalResult = VerticalPang(pos);
        bool horizontalResult = HorizontalPang(pos);

        return verticalResult || horizontalResult;
    }

    public void SelectObject(Pos pos)
    {
        if (state != State.Playing)
            return;

        if (firstSelect == null)
        {
            firstSelect = pos;
            firstSelectBorder.transform.localPosition = board.GetSpawnCoord(pos);
            firstSelectBorder.Init();
        }
        else if (secondSelect == null)
        {
            secondSelect = pos;
            secondSelectBorder.transform.localPosition = board.GetSpawnCoord(pos);
            secondSelectBorder.Init();

            if (firstSelect == secondSelect)
            {
                firstSelect = null;
                secondSelect = null;
                firstSelectBorder.Clear();
                secondSelectBorder.Clear();
                return;
            }

            // ù��° ���� ��� �������� �����¿찡 �ƴϸ� ���� ���
            if (CheckNear() == false)
            {
                firstSelect = null;
                secondSelect = null;
                firstSelectBorder.Clear();
                secondSelectBorder.Clear();
                return;
            }

            StartCoroutine(CoSelectDone());
        }
    }

    public bool IsPang(Pos pos)
    {
        if (CheckVerticalCanPang(pos) == true)
            return true;
        else if (CheckHorizontalCanpang(pos) == true)
            return true;

        return false;
    }

    private void ClearCheckFlag()
    {
        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            for (int j = 0; j < board.BlockHorizontalSize; j++)
            {
                if (isCheck[i][j] == true)
                    isCheck[i][j] = false;
            }
        }
    }

    private bool CheckNear()
    {
        if (firstSelect.y + 1 == secondSelect.y && firstSelect.x == secondSelect.x)
            return true;
        if (firstSelect.y - 1 == secondSelect.y && firstSelect.x == secondSelect.x)
            return true;
        if (firstSelect.y == secondSelect.y && firstSelect.x + 1 == secondSelect.x)
            return true;
        if (firstSelect.y == secondSelect.y && firstSelect.x - 1 == secondSelect.x)
            return true;

        return false;
    }

    private IEnumerator CoSelectDone()
    {
        board.SwapBlock(firstSelect, secondSelect);
        
        yield return new WaitForSeconds(0.4f);

        State = State.Checking;

        firstSelectBorder.Clear();
        secondSelectBorder.Clear();

        bool firstResult = Pang(firstSelect);
        bool secondResult = Pang(secondSelect);

        // �ٲ�ٰ� ��ġ �ȵǸ� �ٽ� ���������
        if (firstResult == false && secondResult == false)
        {
            board.SwapBlock(firstSelect, secondSelect);
            firstSelect = null;
            secondSelect = null;
            State = State.Playing;
        }
        else
        {
            StartCoroutine(CoRefill());
        }
    }

    public IEnumerator CoRefill()
    {
        Debug.Log("CoRefill ȣ��");
        while(breakAnimationCoroutineCount > 0)
        {
            Debug.Log(breakAnimationCoroutineCount);
            yield return null;
        }

        board.Refill();

        yield return new WaitForSeconds(0.2f);

        bool result = PangEntireBoard();
        if (result == true)
        {
            StartCoroutine(CoRefill());
        }
        else
        {
            firstSelect = null;
            secondSelect = null;

            if (CheckSwapPang() == true)
            {
                Debug.Log("�����ؼ� ���� ��� ����");
                State = State.Playing;
            }
            else
            {
                Debug.Log("!! ��� �����ص� �� �ȵ� !!");
                StartCoroutine(CoResetBoard());
            }
        }
    }

    // Three��ġ ���� �״�� ����ϴ� �� ���� ��. Ȯ���� ���̽��� �ʹ� ����
    public bool CheckSwapPang()
    {
        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            for (int j = 0; j < board.BlockHorizontalSize; j++)
            {
                Pos curPos = new Pos(i, j);

                // ���� �ű�ٸ�
                if (i - 1 >= 0)
                {
                    Pos upperPos = new Pos(i - 1, j);
                    board.SwapData(curPos, upperPos);
                    bool result = IsPang(upperPos);
                    board.SwapData(curPos, upperPos);

                    if (result == true)
                        return true;
                }
                // �Ʒ��� �ű�ٸ�
                if (i + 1 < board.BlockVerticalSize)
                {
                    Pos lowerPos = new Pos(i + 1, j);
                    board.SwapData(curPos, lowerPos);
                    bool result = IsPang(lowerPos);
                    board.SwapData(curPos, lowerPos);

                    if (result == true)
                        return true;
                }
                // �������� �ű�ٸ�
                if (j - 1 >= 0)
                {
                    Pos leftPos = new Pos(i, j - 1);
                    board.SwapData(curPos, leftPos);
                    bool result = IsPang(leftPos);
                    board.SwapData(curPos, leftPos);

                    if (result == true)
                        return true;
                }
                // ���������� �ű�ٸ�
                if (j + 1 < board.BlockHorizontalSize)
                {
                    Pos rightPos = new Pos(i, j + 1);
                    board.SwapData(curPos, rightPos);
                    bool result = IsPang(rightPos);
                    board.SwapData(curPos, rightPos);

                    if (result == true)
                        return true;
                }
            }
        }
        
        return false;
    }

    private IEnumerator CoResetBoard()
    {
        board.ResetBoard();
        yield return new WaitForSeconds(0.5f);
        State = State.Playing;
    }

    private bool PangEntireBoard()
    {
        bool result = false;

        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            for (int j = 0; j < board.BlockHorizontalSize; j++)
            {
                bool tempResult = Pang(new Pos(i, j));
                if (tempResult == true)
                    result = true;
            }
        }

        return result;
    }

    private bool CheckVerticalCanPang(Pos pos)
    {
        ClearCheckFlag();
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        BlockKind blockKind = board.GetBlock(pos);
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // ���� ���� ����� return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // �̹� 5�� ��ġ�ߴٸ� Ž�� ����
            if (matchData.Count == 5)
                break;

            // 1. �湮 ���� Ȯ��
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // �� ���� �ٸ��� ��������
            if (board.GetBlock(currentPos) != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. ���� Ȯ��
            queue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
            queue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
        }

        // 3�� �̻� ��ġ������ ���η� 2�� �� ��ġ�� �� �ִ��� Ȯ�� �ʿ�
        // ��͵� �Ƹ� ť�� �־�״ٰ� �ϳ��� ���鼭 Ȯ���ϸ� ���� ������ ����
        // �ٵ� 4��, 5���϶��� ã�� �ʿ� ����
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

                    // ���� ���� ����� return
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3�� ��ġ�ߴٸ� Ž�� ����
                    if (newMatchData.Count == 3)
                        break;

                    // 1. �湮 ���� Ȯ��
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // �� ���� �ٸ��� ��������
                    if (board.GetBlock(currentPos) != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. �¿� Ȯ��
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
                }

                // 4������ �߰� ��ġ �������� �������� �ʰ� �ش� �κ��� ��Ž�� ó�����ֱ�
                if (newMatchData.Count < 3)
                {
                    // check ���� �ʱ�ȭ
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3���� �߰� ��Ī������ �� �������� matchData�� �߰� �� �����Ϸ� ����
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

        queue.Clear();

        if (matchData.Count >= 3)
            return true;
        else
            return false;
    }

    private bool CheckHorizontalCanpang(Pos pos)
    {
        ClearCheckFlag();
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        BlockKind blockKind = board.GetBlock(pos);
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // ���� ���� ����� return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // �̹� 5�� ��ġ�ߴٸ� Ž�� ����
            if (matchData.Count == 5)
                break;

            // 1. �湮 ���� Ȯ��
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // �� ���� �ٸ��� ��������
            if (board.GetBlock(currentPos) != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. �¿� Ȯ��
            queue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
            queue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
        }

        // 3�� �̻� ��ġ������ ���η� 2�� �� ��ġ�� �� �ִ��� Ȯ�� �ʿ�
        // ��͵� �Ƹ� ť�� �־�״ٰ� �ϳ��� ���鼭 Ȯ���ϸ� ���� ������ ����
        // �ٵ� 4��, 5���϶��� ã�� �ʿ� ����
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

                    // ���� ���� ����� return
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3�� ��ġ�ߴٸ� Ž�� ����
                    if (newMatchData.Count == 3)
                        break;

                    // 1. �湮 ���� Ȯ��
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // �� ���� �ٸ��� ��������
                    if (board.GetBlock(currentPos) != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. ���� Ȯ��
                    additionalQueue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
                    additionalQueue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
                }

                // 4������ �߰� ��ġ �������� �������� �ʰ� �ش� �κ��� ��Ž�� ó�����ֱ�
                if (newMatchData.Count < 3)
                {
                    // check ���� �ʱ�ȭ
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3���� �߰� ��Ī������ �� �������� matchData�� �߰� �� �����Ϸ� ����
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

        queue.Clear();

        if (matchData.Count >= 3)
            return true;
        else
            return false;
    }

    private bool VerticalPang(Pos pos)
    {
        ClearCheckFlag();
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        BlockKind blockKind = board.GetBlock(pos);
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // ���� ���� ����� return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // �̹� 5�� ��ġ�ߴٸ� Ž�� ����
            if (matchData.Count == 5)
                break;

            if (board.GetBlock(currentPos) == BlockKind.None)
                continue;

            // 1. �湮 ���� Ȯ��
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // �� ���� �ٸ��� ��������
            if (board.GetBlock(currentPos) != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. ���� Ȯ��
            queue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
            queue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
        }

        // 3�� �̻� ��ġ������ ���η� 2�� �� ��ġ�� �� �ִ��� Ȯ�� �ʿ�
        // ��͵� �Ƹ� ť�� �־�״ٰ� �ϳ��� ���鼭 Ȯ���ϸ� ���� ������ ����
        // �ٵ� 4��, 5���϶��� ã�� �ʿ� ����
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

                    // ���� ���� ����� return
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3�� ��ġ�ߴٸ� Ž�� ����
                    if (newMatchData.Count == 3)
                        break;

                    // 1. �湮 ���� Ȯ��
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // �� ���� �ٸ��� ��������
                    if (board.GetBlock(currentPos) != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. �¿� Ȯ��
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
                    additionalQueue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
                }

                // 4������ �߰� ��ġ �������� �������� �ʰ� �ش� �κ��� ��Ž�� ó�����ֱ�
                if (newMatchData.Count < 3)
                {
                    // check ���� �ʱ�ȭ
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3���� �߰� ��Ī������ �� �������� matchData�� �߰� �� �����Ϸ� ����
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

        bool result = false;

        if (matchData.Count >= 3)
        {
            Break(matchData);
            result = true;
        }

        matchData.Remove(pos);
        queue.Clear();

        return result;
    }

    private bool HorizontalPang(Pos pos)
    {
        ClearCheckFlag();
        List<Pos> matchData = new List<Pos>();

        queue.Enqueue(pos);
        isCheck[pos.y][pos.x] = false;
        BlockKind blockKind = board.GetBlock(pos);
        while (queue.Count > 0)
        {
            Pos currentPos = queue.Dequeue();

            // ���� ���� ����� return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // �̹� 5�� ��ġ�ߴٸ� Ž�� ����
            if (matchData.Count == 5)
                break;

            if (board.GetBlock(currentPos) == BlockKind.None)
                continue;

            // 1. �湮 ���� Ȯ��
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // �� ���� �ٸ��� ��������
            if (board.GetBlock(currentPos) != blockKind)
                continue;

            matchData.Add(currentPos);
            isCheck[currentPos.y][currentPos.x] = true;

            // 2. �¿� Ȯ��
            queue.Enqueue(new Pos(currentPos.y, currentPos.x - 1));
            queue.Enqueue(new Pos(currentPos.y, currentPos.x + 1));
        }

        // 3�� �̻� ��ġ������ ���η� 2�� �� ��ġ�� �� �ִ��� Ȯ�� �ʿ�
        // ��͵� �Ƹ� ť�� �־�״ٰ� �ϳ��� ���鼭 Ȯ���ϸ� ���� ������ ����
        // �ٵ� 4��, 5���϶��� ã�� �ʿ� ����
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

                    // ���� ���� ����� return
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3�� ��ġ�ߴٸ� Ž�� ����
                    if (newMatchData.Count == 3)
                        break;

                    // 1. �湮 ���� Ȯ��
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // �� ���� �ٸ��� ��������
                    if (board.GetBlock(currentPos) != blockKind)
                        continue;

                    newMatchData.Add(currentPos);
                    isCheck[currentPos.y][currentPos.x] = true;

                    // 2. ���� Ȯ��
                    additionalQueue.Enqueue(new Pos(currentPos.y - 1, currentPos.x));
                    additionalQueue.Enqueue(new Pos(currentPos.y + 1, currentPos.x));
                }

                // 4������ �߰� ��ġ �������� �������� �ʰ� �ش� �κ��� ��Ž�� ó�����ֱ�
                if (newMatchData.Count < 3)
                {
                    // check ���� �ʱ�ȭ
                    foreach (Pos p in newMatchData)
                        isCheck[p.y][p.x] = false;

                    newMatchData.Clear();
                }

                // 3���� �߰� ��Ī������ �� �������� matchData�� �߰� �� �����Ϸ� ����
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

        bool result = false;

        if (matchData.Count >= 3)
        {
            Break(matchData);
            result = true;
        }

        matchData.Remove(pos);
        queue.Clear();

        return result;
    }

    //��ġ�� Ÿ���� 3�� �̻��̸� ����
    private void Break(List<Pos> matchData)
    {
        Pos basePos = matchData[0];

        foreach (Pos p in matchData)
            StartCoroutine(board.CoBreak(basePos, p));
    }
}
