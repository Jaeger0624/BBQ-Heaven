using cfg;
using UnityEngine;

namespace cfg{
    public static class DirectionExtra{
        public static Direction ToDirection(this Vector2Int vector2Int){
            if (vector2Int.x == 0 && vector2Int.y == 0){
                Debug.LogError("【ToDirection】向量不能为0");
                return Direction.无;
            }
            if (vector2Int.x != 0 && vector2Int.y != 0){
                Debug.LogError("【ToDirection】向量不能同时为非0");
                return Direction.无;
            }
            if (vector2Int.x != 0){
                return vector2Int.x > 0 ? Direction.右 : Direction.左;
            }
            else{
                return vector2Int.y > 0 ? Direction.上 : Direction.下;
            }
        }
    public static Vector2Int ToVector2Int(this Direction direction){
        switch (direction){
            case Direction.上:
                return new Vector2Int(0, 1);
            case Direction.下:
                return new Vector2Int(0, -1);
            case Direction.左:
                return new Vector2Int(-1, 0);
            case Direction.右:
                return new Vector2Int(1, 0);
            default:
                Debug.LogError("【ToVector2Int】方向不能为无");
                return Vector2Int.zero;
        }
    }
}
}