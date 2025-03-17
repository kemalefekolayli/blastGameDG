using UnityEngine;

public class CubeObjectFactory : MonoBehaviour, ObjectFactory<CubeObject1>
{
    public GameObject cubePrefab;

    public IGridObject CreateObject(Vector2 location, Transform parent, float cellSize, GridManager gridManager, Vector2Int gridPos)
    {
        if (cubePrefab == null)
        {
            Debug.LogError("CubeObjectFactory: No prefab assigned!");
            return null;
        }

        GameObject newCube = Instantiate(cubePrefab, new Vector3(location.x, location.y, 0), Quaternion.identity, parent);
        newCube.transform.localScale = Vector3.one * (cellSize * 0.7f);

        IGridObject gridObject = newCube.GetComponent<IGridObject>();
        if (gridObject != null)
        {
            gridObject.Initialize(gridManager, gridPos);
            Debug.Log("Created a cube at " + location);
            return gridObject ;
        }

        Debug.LogError("CubeObjectFactory: Created object does not implement IGridObject!");
        return null;
    }

}
