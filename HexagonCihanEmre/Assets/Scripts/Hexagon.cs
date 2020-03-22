using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;//This is for accessing the DoTween animations

public class Hexagon : MonoBehaviour
{
    Grid grid;

    [SerializeField] GameObject particleEffectPrefab;

    [SerializeField] private int row;
    [SerializeField] private int column;

    [SerializeField] private Color color;
    [SerializeField] private Vector2 lerpPosition;
    [SerializeField] private bool lerp;
    
    private bool bomb;
    private int bombTimer;

    private TextMesh text;
    private SpriteRenderer spriteRenderer;

    //Struct for holding neighbour coordinates
    public struct NeighbourHexes
    {
        public Vector2 up;
        public Vector2 upLeft;
        public Vector2 upRight;
        public Vector2 down;
        public Vector2 downLeft;
        public Vector2 downRight;
    }

    void Start()
    {
        grid = Grid.instance;
        lerp = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (lerp)
        {
            float newX = Mathf.Lerp(transform.position.x, lerpPosition.x, Time.deltaTime * 9f);
            float newY = Mathf.Lerp(transform.position.y, lerpPosition.y, Time.deltaTime * 9f);
            transform.position = new Vector2(newX, newY);


            if (Vector3.Distance(transform.position, lerpPosition) < 0.05f)
            {
                transform.position = lerpPosition;
                lerp = false;
            }
        }
    }

    //Method for creating a explosion effect matching the hexagons color
    public void Explode()
    {
        ParticleSystem.MainModule particleSettings = particleEffectPrefab.GetComponent<ParticleSystem>().main;
       
        particleSettings.startColor = GetColor();
        Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
    }

    //Function to save rotate changes
    public void Rotate(int newRow, int newColumn, Vector2 newPos)
    {
        lerpPosition = newPos;
        SetRow(newRow);
        SetColumn(newColumn);
        lerp = true;
    }

    //Returns the lerp status 
    public bool IsRotating()
    {
        return lerp;
    }

    //Building a struct from grid position of neighbour hexagons and returns it 
    public NeighbourHexes GetNeighbours()
    {
        NeighbourHexes neighbours;
        bool isEven = grid.IsEven(GetRow());


        neighbours.down = new Vector2(row, column - 1);
        neighbours.up = new Vector2(row, column + 1);
        neighbours.upLeft = new Vector2(row - 1, isEven ? column + 1 : column);
        neighbours.upRight = new Vector2(row + 1, isEven ? column + 1 : column);
        neighbours.downLeft = new Vector2(row - 1, isEven ? column : column - 1);
        neighbours.downRight = new Vector2(row + 1, isEven ? column : column - 1);

        return neighbours;
    }

    //Set new world position for hexagon 
    public void ChangeWorldPosition(Vector2 newPosition)
    {
        lerpPosition = newPosition;
        lerp = true;
    }

    //Set new grid position for hexagon
    public void ChangeGridPosition(Vector2 newPosition)
    {
        row = (int)newPosition.x;
        column = (int)newPosition.y;
    }

    //Setting this hexagon as bomb and setting its properties
    public void SetBomb()
    {
        text = new GameObject().AddComponent<TextMesh>();
        text.alignment = TextAlignment.Center;
        text.anchor = TextAnchor.MiddleCenter;
        text.transform.localScale = new Vector3(0.6f,0.6f,0.6f);
        text.transform.position = new Vector3(transform.position.x, transform.position.y, -4);
        text.color = Color.black;
        
        text.transform.parent = transform;
        bombTimer = 6;
        text.text = bombTimer.ToString();
    }

    //Method for bomb indication animation
    public IEnumerator PlayBombAlertAnimCoroutine()
    {
        
        yield return new WaitForSeconds(0.85f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
        yield return new WaitForSeconds(0.4f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
        yield return new WaitForSeconds(0.4f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
        yield return new WaitForSeconds(0.4f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
        yield return new WaitForSeconds(0.4f);
        transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
        yield return new WaitForSeconds(0.4f);
        transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f);
    }

    //Special version of ChangeWorldPosition adds a delay to the method
    public IEnumerator WaitToChangePosition(float t, Vector3 newPos)
    {
        yield return new WaitForSeconds(t);
        ChangeWorldPosition(newPos);
    }

    //Setting hexagons row index
    public void SetRow(int _row)
    {
        row = _row;
    }

    //Setting hexagons column index
    public void SetColumn(int _column)
    {
        column = _column;
    }

    //Method for setting hexagons color to a value
    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
        color = newColor;
    }

    //Counting down the bomb counter
    public void Tick()
    {
        --bombTimer; text.text = bombTimer.ToString();
    }

    //Getting hexagons row index
    public int GetRow()
    {
        return row;
    }

    //Getting hexagons column index
    public int GetColumn()
    {
        return column;
    }

    //Method for getting hexagons color
    public Color GetColor()
    {
        return GetComponent<SpriteRenderer>().color;
    }

    //Accessing bomb's timer counter
    public int GetTimer()
    {
        return bombTimer;
    }
}
