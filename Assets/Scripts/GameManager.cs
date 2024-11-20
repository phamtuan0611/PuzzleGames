using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static int Rows;
    public static int Cols;

    [SerializeField] private Level _currentlevelData;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _movesText;
    [SerializeField] private TMP_Text _bestText;
    [SerializeField] private Transform _playButtonTransform;
    [SerializeField] private Transform _gridParent;
    [SerializeField] private Transform _nextButtonTransform;
    [SerializeField] private Cell _cellPrefab;

    private int levelNum;
    private int moveNum;
    private int bestNum;

    private bool hasGameStarted;
    private bool hasGameFinished;
    private bool canMove;
    private bool canStartClicking;

    private Tween playStartTween;
    private Tween playNextTween;

    private Cell[,] cells;
    private Color[,] correctColors;

    private Cell selectedCell;
    private Cell movedCell;
    private Vector2 startPos;

    private void Awake()
    {
        instance = this;
        hasGameFinished = false;
        canMove = false;
        canStartClicking = false;
        hasGameStarted = false;
        
    }

    private void Start()
    {
        Rows = _currentlevelData.Row;
        Cols = _currentlevelData.Col;

        //Hien thi Text
        levelNum = PlayerPrefs.GetInt("Level", 1); //Mac dinh la 1 neu khong co du lieu
        moveNum = 0;
        bestNum = PlayerPrefs.GetInt("Best" + levelNum.ToString(), 0); //Lay so diem tot nhat tu LEVEL HIEN TAI, mac dinh la 0 neu khong co du lieu
        _levelText.text = levelNum.ToString();
        _movesText.text = moveNum.ToString();
        _bestText.text = bestNum.ToString();

        DOTween.defaultAutoPlay = AutoPlay.None; //Cac hieu ung DOTWeen khong tu dong chay, phai duoc chi dinh moi chay

        //Button Play
        playStartTween = _playButtonTransform.DOScale(_playButtonTransform.localScale * 1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        //DOScale làm cho đối tượng thay đổi kích thước
        //SetEase(Ease.Linear) chỉ định kiểu chuyển động tuyến tính
        //SetLoops(-1, LoopType.Yoyo) cho phép hiệu ứng lặp lại vô hạn.

        playStartTween.Play();

        //Sinh Cells
        SpawnCells();
    }

    private void Update()
    {
        if (hasGameFinished) return;

        if (!hasGameStarted) return;

        //Check xem da thuc hien het Animation chua, neu roi thi cho phep Start
        if (!canStartClicking)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (cells[i, j].IsStartMovePlaying) return;
                }
            }
            canStartClicking = true;
            canMove = true;
        }
        
        //Doi selectedCell va movedCell hoan thanh di chuyen
        if (!canMove)
        {
            if (selectedCell.hasSelectedMoveFinished && movedCell.hasMoveFinished)
            {
                selectedCell = null;
                movedCell = null;
                canMove = true;
                CheckWin();
            }
        }

        //
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit && hit.collider.TryGetComponent(out selectedCell))
            {
                if (_currentlevelData.LockerCells.Contains(new Vector2Int(selectedCell.Position.y, selectedCell.Position.x)))
                {
                    selectedCell = null;
                    return;
                }
                startPos = mousePos2D;
                selectedCell.SelectedMoveStart();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (selectedCell == null) return;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            Vector2 offset = mousePos2D - startPos;
            selectedCell.SelectedMove(offset);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectedCell == null) return;

            canMove = false;
            Vector2 pos = (Vector2)selectedCell.gameObject.transform.localPosition + new Vector2(0.5f, 0.5f);
            int row = (int)pos.y;
            int col = (int)pos.x;
            movedCell = cells[row, col];
            if (_currentlevelData.LockerCells.Contains(new Vector2Int(row, col)) || movedCell == selectedCell)
            {
                selectedCell.SelectedMoveEnd();
                return;
            }

            Vector2Int tempPos = selectedCell.Position;
            selectedCell.Position = movedCell.Position;
            movedCell.Position = tempPos;

            cells[selectedCell.Position.y, selectedCell.Position.x] = selectedCell;
            cells[movedCell.Position.y, movedCell.Position.x] = movedCell;

            selectedCell.SelectedMoveEnd();
            movedCell.MoveEnd();

            moveNum++;
            _movesText.text = moveNum.ToString();
        }
    }

    private void CheckWin()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (cells[i, j].Color != correctColors[i, j])
                {
                    return;
                }
            }
        }

        hasGameFinished = true;
        if (bestNum == 0 || bestNum > moveNum)
        {
            bestNum = moveNum;
        }
        PlayerPrefs.SetInt("Best" + levelNum.ToString(), bestNum);
        _bestText.text = bestNum.ToString();
        PlayerPrefs.SetInt("Level", levelNum + 1);

        _nextButtonTransform.gameObject.SetActive(true);
        playNextTween = _nextButtonTransform.DOScale(1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        for (int i = 0; i< Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                cells[i, j].GameFinished();
            }
        }
    }

    public void ClickedNextButton()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (cells[i, j].IsStartMovePlaying) return;
            }
        }

        playNextTween.Kill();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void SpawnCells()
    {
        cells = new Cell[_currentlevelData.Row, _currentlevelData.Col];
        correctColors = new Color[_currentlevelData.Row, _currentlevelData.Col];

        Camera.main.backgroundColor = _currentlevelData.BackGroundColor;

        for (int x = 0; x < Rows; x++)
        {
            for (int y = 0; y < Cols; y++)
            {
                float xLerp = (float)y / (Cols - 1);
                float yLerp = (float)x / (Rows - 1);

                //Lefp la ham chon mau
                //Neu yLerp = 0 thi mau se la _currentlevelData.BottomLeftColor
                //Neu yLerp = 1 thi mau se la _currentlevelData.TopLeftColor
                //Neu 0 < yLerp < 1 thi mau se la mot mau pha tron giua 2 mau kia theo ti le yLerp
                Color leftColor = Color.Lerp(_currentlevelData.BottomLeftColor, _currentlevelData.TopLeftColor, yLerp);
                Color rightColor = Color.Lerp(_currentlevelData.BottomRightColor, _currentlevelData.TopRightColor, yLerp);
                Color currentColor = Color.Lerp(leftColor, rightColor, xLerp); //Kieu se lay mau cua doc o 2 ben ngoai truoc, xong moi toi 6 hang doc o giua

                correctColors[x, y] = currentColor;
                cells[x, y] = Instantiate(_cellPrefab, _gridParent);
                cells[x, y].Init(currentColor, y, x); //De y, x thay vi x, y nhu Ham boi vi neu de x, y thi mau se bi xoay ngang
            }
        }
    }

    public void ClickedPlayButton()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Rows; j++)
            {
                if (cells[i, j].IsStartTweenPlaying)
                {
                    return;
                }
            }
        }

        playStartTween.Kill(); //Dung hieu ung Tween
        playStartTween = null; //Giai phong bo nho

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (_currentlevelData.LockerCells.Contains(new Vector2Int(i, j)))
                {
                    continue; //Bo qua cac Block bi khoa
                }

                int swapX, swapY;
                //Xac dinh toa do Swap, lap lai cho den khi khac toa do cua vi tri cac Block bi khoa
                do
                {
                    swapX = Random.Range(0, Rows);
                    swapY = Random.Range(0, Cols);
                } while (_currentlevelData.LockerCells.Contains(new Vector2Int(swapX, swapY)));

                //Thuc hien hoan doi
                Cell temp = cells[i, j];
                cells[i, j] = cells[swapX, swapY];
                Vector2Int swappedPosition = cells[swapX, swapY].Position;
                cells[i, j].Position = temp.Position;
                cells[swapX, swapY] = temp;
                temp.Position = swappedPosition;
            }
        }

        //Thuc thi hieu ung chuyen dong cua cac cell
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                cells[i, j].AniamteStartPositon();
            }
        }

        hasGameStarted = true;
        _playButtonTransform.gameObject.SetActive(false);
    }
}
