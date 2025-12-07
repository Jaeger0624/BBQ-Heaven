using UnityEngine;

public class GridTest : MonoBehaviour
{
    public IGridView gridShower;
    private GridBase grid;
    private bool isChange = false;
    void Start()
    {
        GameObject gridShowerObject = new GameObject("GridShower");
        gridShower = gridShowerObject.AddComponent<GridView>();
        grid = new Grid<Integer>(6, 6, () => new Integer(0));
        gridShower.SetGrid(grid);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isChange = !isChange;
            if (isChange)
            {
                grid.ChangeGrid(4, 4);
            }
            else
            {
                grid.ChangeGrid(6, 6);
            }
            gridShower.SetGrid(grid);
        }
    }
}
