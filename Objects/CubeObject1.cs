using UnityEngine;

public enum ObjectColor { Red, Green, Blue, Yellow }

public class CubeObject1 : MonoBehaviour, IGridObject, IFallable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private ObjectColor cubeColor;
    private GridManager gridManager;
    private Vector2Int gridPosition;


    public void DoWork(){
    Debug.Log("zort");
    }
    public void Initialize(GridManager gridManager, Vector2Int position)
        {

            this.gridManager =  gridManager;
            this.gridPosition = position;
        }


    void Start()
    {


    }

    void OnMouseDown()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager is NULL in CubeObject!");
            return;
        }
        gridManager.RemoveCube(gridPosition); // Delegate removal to GridManager
    }

    public void HandleFalling(){
    }



    // Update is called once per frame
    void Update()
    {
        
    }



    public ObjectColor getColor()
        {
            return cubeColor;
        }
    public void setColor(string color)
        {
            if (System.Enum.TryParse(color, true, out ObjectColor parsedColor))
            {
                cubeColor = parsedColor;
            }
            else
            {
                Debug.LogWarning("Invalid color: ");
            }
        }

}
