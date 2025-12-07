using UnityEngine;

public static class AnimUtility{
    public const float TextSpawnZOffset = -10f;
    public static Vector3 GetTextSpawnPosition(Transform transform, bool isLeft){
        if (isLeft){
            return transform.position + new Vector3(-0.3f, 0.3f, TextSpawnZOffset);
        } else {
            return transform.position + new Vector3(0.3f, 0.3f, TextSpawnZOffset);
        }
    }

    public static Vector3 GetTextSpawnPosition(Vector3 position){
        return new Vector3(position.x, position.y, TextSpawnZOffset);
    }
}