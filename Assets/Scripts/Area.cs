using UnityEngine;

namespace DefaultNamespace
{
    public struct Area
    {
        public Vector2 min;
        public Vector2 max;
        public float sqr => (max.x - min.x) * (max.y - min.y);

        public Vector2 size => new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        public Vector2 center => new Vector2(min.x + size.x / 2f, min.y + size.y / 2f);

        public bool isBadArea => sqr <= 1;

        public bool Contains(Vector2 point)
        {
            return (min.x <= point.x && max.x >= point.x) && (min.y <= point.y && max.y >= point.y);
        }
    }
}