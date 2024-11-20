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
    private Cell moveCell;
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

        //playStartTween.Kill();
        //playStartTween = null;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                if (_currentlevelData.LockerCells.Contains(new Vector2Int(i, j))){
                    continue;
                }

                int swapX, swapY;
                do
                {
                    swapX = Random.Range(0, Rows);
                    swapY = Random.Range(0, Cols);
                } while (_currentlevelData.LockerCells.Contains(new Vector2Int(swapX, swapY)));

                Cell temp = cells[i, j];
                cells[i, j] = cells[swapX, swapY];
                Vector2Int swappedPosition = cells[swapX, swapY].Position;
                cells[i, j].Position = temp.Position;
                cells[swapX, swapY] = temp;
                temp.Position = swappedPosition;
            }
        }

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
