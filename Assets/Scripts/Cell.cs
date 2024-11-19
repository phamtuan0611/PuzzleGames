using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [HideInInspector] public Color Color;
    [HideInInspector] public Vector2Int Position;

    [SerializeField] private SpriteRenderer _bgSprite;

    public void Init(Color color, int x, int y)
    {
        Debug.Log($"Init called with Color: {color}, Position: ({x}, {y})");
        Color = color;
        _bgSprite.color = Color;
        Position = new Vector2Int(x, y);
        transform.localPosition = new Vector3(x, y, 0);
    }
}
