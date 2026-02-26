using System.Collections.Generic;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public static class HistoryTrimmer
    {
        public static bool TrimIfNeeded<T>(List<T> list, int maxCount, int trimToCount)
        {
            if (list == null || maxCount <= 0 || list.Count <= maxCount) return false;

            if (trimToCount < 0) trimToCount = 0;
            if (trimToCount > maxCount) trimToCount = maxCount;

            int itemsToRemove = list.Count - trimToCount;
            if (itemsToRemove > 0)
            {
                list.RemoveRange(0, itemsToRemove);
                return true;
            }
            return false;
        }

        public static bool TrimGameObjectsIfNeeded(List<GameObject> gameObjects, int maxCount, int trimToCount)
        {
            if (gameObjects == null || maxCount <= 0 || gameObjects.Count <= maxCount) return false;

            if (trimToCount < 0) trimToCount = 0;
            if (trimToCount > maxCount) trimToCount = maxCount;

            int itemsToRemove = gameObjects.Count - trimToCount;
            if (itemsToRemove > 0)
            {
                for (int i = 0; i < itemsToRemove; i++)
                {
                    if (gameObjects[i] != null)
                        Object.Destroy(gameObjects[i]);
                }
                gameObjects.RemoveRange(0, itemsToRemove);
                return true;
            }
            return false;
        }
    }
}
