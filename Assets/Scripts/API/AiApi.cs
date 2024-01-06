using UnityEngine;

namespace AI.API
{
    internal static class AiApi
    {
        public static float aiDelta = 0.1f;

        public const string playerTag = "Player";
        public const string carTag = "Car";
        private const float maxRadius = 100f;

        public static bool GetClosestCarPosition(Vector2 origin, out Vector2 position, LayerMask mask)
        {
            float minimalDistance = float.MaxValue;
            position = Vector2.zero;
            var state = false;

            var collider = Physics2D.OverlapCircleAll(origin, maxRadius, mask.value);

            for (int i = 0; i < collider.Length; i++)
            {
                if (!collider[i].CompareTag(carTag))
                    continue;

                if (collider[i].GetComponent<CarControl>().Occupied)
                    continue;

                float distance = Vector2.Distance(collider[i].transform.position, origin);

                if (distance < minimalDistance)
                {
                    minimalDistance = distance;
                    position = collider[i].transform.position;

                    state = true;
                }
            }

            return state;
        }
    }
}
