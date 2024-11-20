using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public Color Color;
    [HideInInspector] public Vector2Int Position;

    public bool IsStartTweenPlaying => startAnimation.IsActive();
    public bool IsStartMovePlaying => startMoveAnimation.IsActive();
    public bool hasSelectedMoveFinished => !selectedMoveAnimationTime.IsActive();
    public bool hasMoveFinished => !moveAnimation.IsActive(); 

    [SerializeField] private SpriteRenderer _bgSprite;
    [SerializeField] private float _startScaleDelay = 0.04f;
    [SerializeField] private float _startScaleTime = 0.2f;
    [SerializeField] private float _startMoveAnimationTime = 0.32f;
    [SerializeField] private float _selectedmoveAnimationTime = 0.16f;
    [SerializeField] private float _moveAnimationTime = 0.32f;

    private Tween startAnimation;
    private Tween startMoveAnimation;
    private Tween selectedMoveAnimationTime;
    private Tween moveAnimation;

    private const int FRONT = 1;
    private const int BACK = 0;

    public void Init(Color color, int x, int y)
    {
        Debug.Log($"Init called with Color: {color}, Position: ({x}, {y})");
        Color = color;
        _bgSprite.color = Color;
        Position = new Vector2Int(x, y);
        transform.localPosition = new Vector3(x, y, 0);

        //Hien co su dung Delay
        transform.localScale = Vector3.zero;
        float delay = (x + y) * _startScaleDelay;
        startAnimation = transform.DOScale(1f, _startScaleTime);
        startAnimation.SetEase(Ease.OutExpo); //Hieu ung cham dan khi sap ket thuc
        startAnimation.SetDelay(0.5f + delay);
        startAnimation.Play();
    }

    public void AniamteStartPositon()
    {
        startMoveAnimation = transform.DOLocalMove(new Vector3(Position.x, Position.y, 0), _startMoveAnimationTime);
        startMoveAnimation.SetEase(Ease.InSine); //Hieu ung bat dau cham roi nhanh dan (nguoc lai voi OutExpo
        startMoveAnimation.Play();
    }
}
