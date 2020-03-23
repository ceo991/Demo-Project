using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Grid : MonoBehaviour
{

    public static Grid instance = null;


    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private GameObject outParent;

    [SerializeField] private Sprite outlineSprite;
    [SerializeField] private Sprite hexagonSprite;

    
    [SerializeField] private int gridWidth;
    [SerializeField] private int gridHeight;
    private int selectionStatus;

    [SerializeField] private int explodedCount;
    [SerializeField] private int explodedGroupCount;

    private bool bombProduction;
    private bool gameOver;

    private Vector2 selectedPosition;

    private Hexagon selectedHexagon;

    private List<List<Hexagon>> grid;
    private List<Hexagon> selectedGroup;
    [SerializeField] private List<Hexagon> bombs;

    [SerializeField] private List<Color> hexColorListContainer; //This is for adjusting the color count and values from editor
    private List<Color> hexColorList;

    private bool hexRotation;
    private bool hexExplosion;
    private bool hexProduction;
    private bool validTouch;

    [SerializeField] private float boarderSize;
    
    private Vector2 touchStartPosition;

    UIManager uIManager;
    SoundManager soundManager;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
    }

    void Start()
    {
        gameOver = false;
        bombProduction = false;
        hexRotation = false;
        hexExplosion = false;
        hexProduction = false;
        bombs = new List<Hexagon>();
        selectedGroup = new List<Hexagon>();

        grid = new List<List<Hexagon>>();

        hexColorList = new List<Color>();

        //Filling the actual color list from color list container
        for (int i = 0; i < hexColorListContainer.Count; i++)
        {
            hexColorList.Add(hexColorListContainer[i]);
        }

        SetColorList(hexColorList);

        InitializeGrid();
        SetupCamera();

        uIManager = UIManager.instance;
        soundManager = SoundManager.instance;

    }

    void Update()
    {

        if (InputAvailabile() && Input.touchCount > 0)
        {
            
            //Taking collider of the touched hexagon
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            Collider2D collider = Physics2D.OverlapPoint(touchPos);
            selectedHexagon = GetSelectedHexagon();

            //Receiveing input
            TouchDetection();
            CheckSelection(collider);
            CheckRotation();
        }
    }

    //method for grid initialization
    public void InitializeGrid()
    {
        List<int> missingCells = new List<int>();

        //Initialize gameGrid and fill cells 
        for (int i = 0; i < GetGridWidth(); ++i)
        {
            for (int j = 0; j < GetGridHeight(); ++j)

                missingCells.Add(i);

            grid.Add(new List<Hexagon>());
        }

        
        // Fills the grid with hexagons
        StartCoroutine(ProduceHexagons(missingCells, ColoredGridProducer()));
    }

    //Produces new hexagons
    private IEnumerator ProduceHexagons(List<int> columns, List<List<Color>> colorSeed = null)
    {
        Vector3 posToBeMoved;
        float positionX, positionY;

        hexProduction = true;

        //Produce a new hexagon,and set its variables
        foreach (int i in columns)
        {
            positionX = (0.8f * i);
            positionY = (0.885f * grid[i].Count) + (IsEven(i) ? 0.885f * 0.5f : 0);

            posToBeMoved = new Vector3(positionX, positionY, 0);

            GameObject newObj = Instantiate(hexPrefab, new Vector3(5f, 45f, 0f), Quaternion.Euler(0f,0f,90f), transform);
            newObj.name = "Hex (" + i + " ," + grid[i].Count + ")";
            Hexagon newHex = newObj.GetComponent<Hexagon>();
            yield return new WaitForSeconds(0.025f);

            //Seting a bomb if condition is met 
            if (bombProduction)
            {
                soundManager.PlayBombAlert();
                newHex.SetBomb();
                bombs.Add(newHex);

                StartCoroutine(newHex.PlayBombAlertAnimCoroutine());
                                
                bombProduction = false;
            }

            if (colorSeed == null)
                newHex.SetColor(hexColorList[Random.Range(0, hexColorList.Count)]);
            else
                newHex.SetColor(colorSeed[i][grid[i].Count]);

            newHex.ChangeGridPosition(new Vector2(i, grid[i].Count));
            newHex.ChangeWorldPosition(posToBeMoved);
            grid[i].Add(newHex);
        }

        hexProduction = false;

    }

    //Function to produce a grid with colors
    private List<List<Color>> ColoredGridProducer()
    {
        List<List<Color>> returnValue = new List<List<Color>>();
        List<Color> checkList = new List<Color>();
        bool exit = true;


        //Creating colors and making sure no hexagon is matched
        for (int i = 0; i < GetGridWidth(); ++i)
        {
            returnValue.Add(new List<Color>());
            for (int j = 0; j < GetGridHeight(); ++j)
            {
                returnValue[i].Add(hexColorList[Random.Range(0, hexColorList.Count)]);
                do
                {
                    exit = true;
                    returnValue[i][j] = hexColorList[Random.Range(0, hexColorList.Count)];
                    if (i - 1 >= 0 && j - 1 >= 0)
                    {
                        if (returnValue[i][j - 1] == returnValue[i][j] || returnValue[i - 1][j] == returnValue[i][j])
                            exit = false;
                    }
                } while (!exit);
            }
        }


        return returnValue;
    }

    //Setting up camera to center the grid 
    //and making sure grid is completely visible
    public void SetupCamera()
    {
        Camera.main.transform.position = new Vector3(((float)gridWidth -1 ) * 0.8f / 2, ((float)gridHeight -1) * 0.885f / 2 , -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;

        float verticalSize = ((float)gridHeight / 2 + (float)boarderSize) * 0.885f;

        float horizontalSize = (((float)gridWidth / 2 + (float)boarderSize) / aspectRatio) * 0.8f;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    //Selecting the hex group on touch position 
    public void Select(Collider2D collider)
    {
        if (selectedHexagon == null || !selectedHexagon.GetComponent<Collider2D>().Equals(collider))
        {
            selectedHexagon = collider.gameObject.GetComponent<Hexagon>();
            selectedPosition.x = selectedHexagon.GetRow();
            selectedPosition.y = selectedHexagon.GetColumn();
            selectionStatus = 0;
        }
        else
        {
            selectionStatus = (++selectionStatus) % 6;
        }

        DestructOutline();
        ConstructOutline();
    }

    //Rotates the hexagon group
    public void Rotate(bool clockWise)
    {
        DestructOutline();
        StartCoroutine(RotationCheckCoroutine(clockWise));
    }

    //Finds all 3 hexagons to be outlined 
    private void FindHexagonGroup()
    {
        List<Hexagon> returnValue = new List<Hexagon>();
        Vector2 firstPos, secondPos;

        //Finding 2 other required hexagon
        selectedHexagon = grid[(int)selectedPosition.x][(int)selectedPosition.y];
        FindOtherHexagons(out firstPos, out secondPos);
        selectedGroup.Clear();
        selectedGroup.Add(selectedHexagon);
        selectedGroup.Add(grid[(int)firstPos.x][(int)firstPos.y].GetComponent<Hexagon>());
        selectedGroup.Add(grid[(int)secondPos.x][(int)secondPos.y].GetComponent<Hexagon>());
    }

    //function for FindHexagonGroup() to locate neighbours of selected hexagon 
    private void FindOtherHexagons(out Vector2 first, out Vector2 second)
    {
        Hexagon.NeighbourHexes neighbours = selectedHexagon.GetNeighbours();
        bool breakLoop = false;

        do
        {
            switch (selectionStatus)
            {
                case 0:
                     first = neighbours.up;
                     second = neighbours.upRight;
                     break;
                case 1:
                     first = neighbours.upRight;
                     second = neighbours.downRight;
                     break;
                case 2:
                     first = neighbours.downRight;
                     second = neighbours.down;
                     break;
                case 3:
                     first = neighbours.down;
                     second = neighbours.downLeft;
                     break;
                case 4:
                     first = neighbours.downLeft;
                     second = neighbours.upLeft;
                     break;
                case 5:
                     first = neighbours.upLeft;
                     second = neighbours.up;
                     break;
                default:
                     first = Vector2.zero;
                     second = Vector2.zero;
                     break;
            }

            //Loop until two neighbours with valid positions are found 
            if (first.x < 0 || first.x >= gridWidth || first.y < 0 || first.y >= gridHeight || second.x < 0 || second.x >= gridWidth || second.y < 0 || second.y >= gridHeight)
            {
                selectionStatus = (++selectionStatus) % 6;
            }
            else
            {
                breakLoop = true;
            }
        } while (!breakLoop);
    }

    //Function to check if all hexagons finished rotating
    private IEnumerator RotationCheckCoroutine(bool clockWise)
    {
        List<Hexagon> explosiveHexagons = null;
        List<int> hexagonsToExplode = null;
        bool flag = true;

        hexRotation = true;
        for (int i = 0; i < selectedGroup.Count; ++i)
        {
            //Swaps hexagons and waits for some amount
            SwapHexagons(clockWise);
            yield return new WaitForSeconds(0.5f);

            // Check if there is any explosion available
            explosiveHexagons = CheckExplosion(grid);
            if (explosiveHexagons.Count > 0)
            {
                uIManager.UpdateTurn();
                explodedCount = 0;
                explodedGroupCount = 0;
                break;
            }
        }

        hexExplosion = true;
        hexRotation = false;


        //Explodes the hexagons
        while (explosiveHexagons.Count > 0)
        {
            if (flag)
            {
                hexProduction = true;
                hexagonsToExplode = ExplodeHexagons(explosiveHexagons);
                yield return new WaitForSeconds(0.3f);
                StartCoroutine(ProduceHexagons(hexagonsToExplode));
                flag = false;
            }
            else if (!hexProduction)
            {
                explosiveHexagons = CheckExplosion(grid);
                
                flag = true;
            }

            yield return new WaitForSeconds(0.5f);
        }

        hexExplosion = false;
        FindHexagonGroup();
        ConstructOutline();
    }

    private void SwapHexagons(bool clockWise)
    {
        int x1, x2, x3, y1, y2, y3;
        Vector2 pos1, pos2, pos3;
        Hexagon first, second, third;

        soundManager.PlaySwipeEffect();

        //Taking each position to local variables to prevent data loss during rotation
        first = selectedGroup[0];
        second = selectedGroup[1];
        third = selectedGroup[2];

        x1 = first.GetRow();
        x2 = second.GetRow();
        x3 = third.GetRow();

        y1 = first.GetColumn();
        y2 = second.GetColumn();
        y3 = third.GetColumn();

        pos1 = first.transform.position;
        pos2 = second.transform.position;
        pos3 = third.transform.position;


        //If rotation is clokwise, rotate to the position of element on next index, else rotate to previous index 
        if (clockWise)
        {
            first.Rotate(x2, y2, pos2);
            grid[x2][y2] = first;

            second.Rotate(x3, y3, pos3);
            grid[x3][y3] = second;

            third.Rotate(x1, y1, pos1);
            grid[x1][y1] = third;
        }
        else
        {
            first.Rotate(x3, y3, pos3);
            grid[x3][y3] = first;

            second.Rotate(x1, y1, pos1);
            grid[x1][y1] = second;

            third.Rotate(x2, y2, pos2);
            grid[x2][y2] = third;
        }
    }

    //Returns a list that contains hexagons which are ready to explode, returns an empty list if there is none 
    private List<Hexagon> CheckExplosion(List<List<Hexagon>> listToCheck)
    {
        List<Hexagon> neighbourList = new List<Hexagon>();
        List<Hexagon> explosiveList = new List<Hexagon>();
        Hexagon currentHexagon;
        Hexagon.NeighbourHexes currentNeighbours;
        Color currentColor;


        for (int i = 0; i < listToCheck.Count; ++i)
        {
            for (int j = 0; j < listToCheck[i].Count; ++j)
            {
                currentHexagon = listToCheck[i][j];
                currentColor = currentHexagon.GetColor();
                currentNeighbours = currentHexagon.GetNeighbours();

                //Fill neighbour list with up-upright-downright neighbours with valid positions 
                if (IsNeighbourValid(currentNeighbours.up))
                {
                    neighbourList.Add(grid[(int)currentNeighbours.up.x][(int)currentNeighbours.up.y]);
                }
                else
                {
                    neighbourList.Add(null);
                }

                if (IsNeighbourValid(currentNeighbours.upRight))
                {
                    neighbourList.Add(grid[(int)currentNeighbours.upRight.x][(int)currentNeighbours.upRight.y]);
                }
                else
                {
                    neighbourList.Add(null);
                }

                if (IsNeighbourValid(currentNeighbours.downRight))
                {
                    neighbourList.Add(grid[(int)currentNeighbours.downRight.x][(int)currentNeighbours.downRight.y]);
                }
                else
                {
                    neighbourList.Add(null);
                }


                //If current 3 hexagons are all same color then add them to explosion list 
                for (int k = 0; k < neighbourList.Count - 1; ++k)
                {
                    if (neighbourList[k] != null && neighbourList[k + 1] != null)
                    {
                        if (neighbourList[k].GetColor() == currentColor && neighbourList[k + 1].GetColor() == currentColor)
                        {
                            if (!explosiveList.Contains(neighbourList[k]))
                            {
                                explosiveList.Add(neighbourList[k]);
                            }

                            if (!explosiveList.Contains(neighbourList[k + 1]))
                            {
                                explosiveList.Add(neighbourList[k + 1]);
                            }

                            if (!explosiveList.Contains(currentHexagon))
                            {
                                explosiveList.Add(currentHexagon);
                            }
                        }
                    }
                }

                neighbourList.Clear();
            }
        }


        return explosiveList;
    }

    //Function to clear explosive hexagons and tidy up the grid 
    private List<int> ExplodeHexagons(List<Hexagon> list)
    {
        List<int> missingColumns = new List<int>();
        float positionX, positionY;


        //Check for bombs
        foreach (Hexagon hex in bombs)
        {
            if (!list.Contains(hex))
            {
                if (explodedGroupCount < 1)
                {
                    hex.Tick();
                }

                if (hex.GetTimer() == 0)
                {
                    gameOver = true;
                    uIManager.GameOver();
                    StopAllCoroutines();         
                    return missingColumns;
                }
            }
        }

        //Remove hexagons from game grid
        foreach (Hexagon hex in list) {
        	if (bombs.Contains(hex)) {
        		bombs.Remove(hex);
               StartCoroutine(soundManager.PlayBombDestroyedNotifyCoroutine());
        	}
            soundManager.PlayExplosion();
            uIManager.UpdateScore(1);
        	grid[hex.GetRow()].Remove(hex);
        	missingColumns.Add(hex.GetRow());            
        	Destroy(hex.gameObject);
            hex.Explode();
            ++explodedCount; 
        }

        //Count color matched hexagon group explosions
        explodedGroupCount = Mathf.RoundToInt(explodedCount / list.Count);

        foreach (int i in missingColumns)
        {
            for (int j = 0; j < grid[i].Count; ++j)
            {
                positionX = (0.8f * i);
                positionY = (0.885f * j)  + (IsEven(i) ? 0.885f * 0.5f : 0f);            
                grid[i][j].SetColumn(j);
                grid[i][j].SetRow(i);
                StartCoroutine(grid[i][j].WaitToChangePosition(0.3f,new Vector3(positionX, positionY, 0)));
            }
        }

        hexExplosion = false;
        return missingColumns;
    }

    //Clears the outline objects
    private void DestructOutline()
    {
        if (outParent.transform.childCount > 0)
        {
            foreach (Transform child in outParent.transform)
                Destroy(child.gameObject);
        }
    }

    //Builds outline
    private void ConstructOutline()
    {
        FindHexagonGroup();

        foreach (Hexagon outlinedHexagon in selectedGroup)
        {
            GameObject go = outlinedHexagon.gameObject;
            GameObject outline = new GameObject("Outline");
            GameObject outlineInner = new GameObject("Inner Object");

            outline.transform.parent = outParent.transform;

            outline.AddComponent<SpriteRenderer>();
            outline.GetComponent<SpriteRenderer>().sprite = outlineSprite;
            outline.transform.eulerAngles = new Vector3(0f, 0f, 90f);
            outline.GetComponent<SpriteRenderer>().color = Color.white ;
            outline.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -1);
            outline.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

            outlineInner.AddComponent<SpriteRenderer>();
            outlineInner.GetComponent<SpriteRenderer>().sprite = hexagonSprite;
            outlineInner.transform.eulerAngles = new Vector3(0f, 0f, 90f);
            outlineInner.GetComponent<SpriteRenderer>().color = go.GetComponent<SpriteRenderer>().color;
            outlineInner.transform.position = new Vector3(go.transform.position.x, go.transform.position.y, -2);
            outlineInner.transform.localScale = new Vector3(0.8f,0.8f,0.8f);
            outlineInner.transform.parent = outline.transform;
        }
    }

    //Findsout if a given number is even
    //This is for determining if a column will be moved up a certain amount
    public bool IsEven(int x)
    {
         return (x % 2 == 0);
    }

    //Method for validating the input
    public bool InputAvailabile()
    {
        return !hexProduction && !gameOver && !hexRotation && !hexExplosion;
    }

    //Method for checking if the neighbour is in bounds
    private bool IsNeighbourValid(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < GetGridWidth() && pos.y >= 0 && pos.y < GetGridHeight();
    }

    //Method for setting the width of the grid
    public void SetGridWidth(int width)
    {
        gridWidth = width;
    }

    // method for setting the height of the grid
    public void SetGridHeight(int height)
    {
        gridHeight = height;
    }

    //Sets the color list
    public void SetColorList(List<Color> list)
    {
        hexColorList = list;
    }

    //Validates bomb production
    public void SetBombProduction()
    {
        bombProduction = true;
    }

    //Method for returning grid width
    public int GetGridWidth()
    {
        return gridWidth;
    }

    //Method for returning grid height
    public int GetGridHeight()
    {
        return gridHeight;
    }

    //Method for returning selected hexagon
    public Hexagon GetSelectedHexagon()
    {
        return selectedHexagon;
    }

    // Checking if the first touch has arrived
    private void TouchDetection()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            validTouch = true;
            touchStartPosition = Input.GetTouch(0).position;
        }
    }

    //Checks if selection condition provided and calls grid manager to handle selection
    private void CheckSelection(Collider2D collider)
    {
        //If there is a collider and its tag match with any Hexagon continue operate 
        if (collider != null && collider.transform.tag == "Hexagon")
        {
            //Select hexagon if touch ended 
            if (Input.GetTouch(0).phase == TouchPhase.Ended && validTouch)
            {
                validTouch = false;
                Select(collider);
            }
        }
    }

    //Checks if rotation condition provided and calls grid manager to handle rotation
    private void CheckRotation()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved && validTouch)
        {
            Vector2 touchCurrentPosition = Input.GetTouch(0).position;
            float distanceX = touchCurrentPosition.x - touchStartPosition.x;
            float distanceY = touchCurrentPosition.y - touchStartPosition.y;

            if ((Mathf.Abs(distanceX) > 10 || Mathf.Abs(distanceY) > 10) && selectedHexagon != null)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(selectedHexagon.transform.position);

                bool triggerOnX = Mathf.Abs(distanceX) > Mathf.Abs(distanceY);
                bool swipeRightUp = triggerOnX ? distanceX > 0 : distanceY > 0;
                bool touchThanHex = triggerOnX ? touchCurrentPosition.y > screenPosition.y : touchCurrentPosition.x > screenPosition.x;
                bool clockWise = triggerOnX ? swipeRightUp == touchThanHex : swipeRightUp != touchThanHex;

                validTouch = false;
                Rotate(clockWise);
            }
        }
    }
}
