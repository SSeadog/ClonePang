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

    private BoardController board;

    private bool[][] isCheck;
    private Queue<Pos> queue = new Queue<Pos>();

    private Pos firstSelect;
    private Pos secondSelect;

    private State state;

    public State State { get { return state; } set { if (state != value) state = value; } }

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

    // bfs로 구현
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
        }
        else if (secondSelect == null)
        {
            if (firstSelect == secondSelect)
            {
                firstSelect = null;
                secondSelect = null;
                return;
            }

            State = State.Checking;
            secondSelect = pos;
            board.SwapBlock(firstSelect, secondSelect);
            CheckPang(firstSelect);
            CheckPang(secondSelect);

            StartCoroutine(CoRefill());
        }
    }

    public IEnumerator CoRefill()
    {
        Debug.Log("CoRefill 호출");
        yield return new WaitForSeconds(1f);
        board.Refill();

        yield return new WaitForSeconds(1f);
        CheckEntireBoard();

        if (IsEmptyExist() == true)
        {
            StartCoroutine(CoRefill());
        }
        else
        {
            firstSelect = null;
            secondSelect = null;
            State = State.Playing;
        }

    }

    private void CheckEntireBoard()
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

            // 보드 범위 벗어나면 return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // 이미 5개 매치했다면 탐색 종료
            if (matchData.Count == 5)
                break;

            // 1. 방문 여부 확인
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // 블럭 색이 다르면 다음으로
            if (board.GetBlock(currentPos) != blockKind)
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
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3개 매치했다면 탐색 종료
                    if (newMatchData.Count == 3)
                        break;

                    // 1. 방문 여부 확인
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // 블럭 색이 다르면 다음으로
                    if (board.GetBlock(currentPos) != blockKind)
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

            // 보드 범위 벗어나면 return
            if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                continue;
            if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                continue;

            // 이미 5개 매치했다면 탐색 종료
            if (matchData.Count == 5)
                break;

            // 1. 방문 여부 확인
            if (isCheck[currentPos.y][currentPos.x] == true)
                continue;

            // 블럭 색이 다르면 다음으로
            if (board.GetBlock(currentPos) != blockKind)
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
                    if (currentPos.y < 0 || currentPos.y >= board.BlockVerticalSize)
                        continue;
                    if (currentPos.x < 0 || currentPos.x >= board.BlockHorizontalSize)
                        continue;

                    // 3개 매치했다면 탐색 종료
                    if (newMatchData.Count == 3)
                        break;

                    // 1. 방문 여부 확인
                    if (isCheck[currentPos.y][currentPos.x] == true)
                        continue;

                    // 블럭 색이 다르면 다음으로
                    if (board.GetBlock(currentPos) != blockKind)
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

        Break(matchData);
        matchData.Remove(pos);
        queue.Clear();
    }

    //매치된 타일이 3개 이상이면 삭제
    private void Break(List<Pos> matchData)
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
            board.BreakBlock(p);
        }
    }




}
