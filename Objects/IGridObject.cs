using UnityEngine;

public interface IGridObject{

    public void DoWork();



    void Initialize(GridManager gridManager, Vector2Int position); //

}

public interface IFallable{

    public  void HandleFalling();
}