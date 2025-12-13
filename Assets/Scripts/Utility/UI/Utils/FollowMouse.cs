using UnityEngine;
public class FollowMouse : MonoBehaviour
{
    [SerializeField] private Vector2 offset = new Vector2(0, 0);
    void Update()
    {
        Follow();
    }
    public void Follow(){
        Vector3 mousePosition = Utility.GetMousePosition2D();
        transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, 0);
    }
}