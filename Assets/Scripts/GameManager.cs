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

        levelNum = PlayerPrefs.GetInt("Level", 1); //Mac dinh la 1 neu khong co du lieu
        moveNum = 0;
        bestNum = PlayerPrefs.GetInt("Best" + levelNum.ToString(), 0); //Lay so diem tot nhat tu LEVEL HIEN TAI, mac dinh la 0 neu khong co du lieu
        _levelText.text = levelNum.ToString();
        _movesText.text = moveNum.ToString();
        _bestText.text = bestNum.ToString();

        DOTween.defaultAutoPlay = AutoPlay.None; //Cac hieu ung DOTWeen khong tu dong chay, phai duoc chi dinh moi chay

        playStartTween = _playButtonTransform.DOScale(_playButtonTransform.localScale * 1.1f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        //DOScale làm cho đối tượng thay đổi kích thước
        //SetEase(Ease.Linear) chỉ định kiểu chuyển động tuyến tính
        //SetLoops(-1, LoopType.Yoyo) cho phép hiệu ứng lặp lại vô hạn.

        playStartTween.Play();

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

                Color leftColor = Color.Lerp(_currentlevelData.BottomLeftColor, _currentlevelData.TopLeftColor, yLerp);
                Color rightColor = Color.Lerp(_currentlevelData.BottomRightColor, _currentlevelData.TopRightColor, yLerp);
                Color currentColor = Color.Lerp(leftColor, rightColor, xLerp);

                correctColors[x, y] = currentColor;
                cells[x, y] = Instantiate(_cellPrefab, _gridParent);
                cells[x, y].Init(currentColor, y, x);
            }
        }
    }
}
