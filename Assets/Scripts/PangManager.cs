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

    // bfs�� ����
    public void CheckPang(Pos pos)
    {
        CheckVertical(pos);
        CheckHorizontal(pos);
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

        CheckPang(firstSelect);
        CheckPang(secondSelect);

        // �ٲ�ٰ� ��ġ �ȵǸ� �ٽ� ���������
        if (IsEmptyExist() == false)
        {
            board.SwapBlock(firstSelect, secondSelect);
            firstSelect = null;
            secondSelect = null;
            State = State.Playing;
        }
        else
        {
            StartCoroutine(CoRefill(0.4f));
        }

    }

    public IEnumerator CoRefill(float waitTime = 1f)
    {
        Debug.Log("CoRefill ȣ��");
        yield return new WaitForSeconds(waitTime);
        board.Refill();

        yield return new WaitForSeconds(1f);
        CheckPangEntireBoard();

        if (IsEmptyExist() == true)
        {
            StartCoroutine(CoRefill(0.4f));
        }
        else
        {
            firstSelect = null;
            secondSelect = null;
            State = State.Playing;
        }

    }

    private void CheckPangEntireBoard()
    {
        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            for (int j = 0; j < board.BlockHorizontalSize; j++)
            {
                PangManager.Instance.CheckPang(new Pos(i, j));
            }
        }
    }

    private bool IsEmptyExist()
    {
        for (int i = 0; i < board.BlockVerticalSize; i++)
        {
            for (int j = 0; j < board.BlockHorizontalSize; j++)
            {
                if (board.GetBlock(new Pos(i,j)) == BlockKind.None)
                    return true;
            }
        }
        return false;
    }

    private void CheckVertical(Pos pos)
    {
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

        Break(matchData);
        matchData.Remove(pos);
        queue.Clear();
    }

    private void CheckHorizontal(Pos pos)
    {
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

        Break(matchData);
        matchData.Remove(pos);
        queue.Clear();
    }

    //��ġ�� Ÿ���� 3�� �̻��̸� ����
    private void Break(List<Pos> matchData)
    {
        if (matchData.Count < 3)
        {
            // check ���� �ʱ�ȭ
            foreach (Pos p in matchData)
                isCheck[p.y][p.x] = false;

            return;
        }

        Debug.Log(matchData.Count + " �� �μ���!");

        foreach (Pos p in matchData)
        {
            Debug.Log(p.y + "," + p.x);
            board.BreakBlock(p);
        }
    }




}
