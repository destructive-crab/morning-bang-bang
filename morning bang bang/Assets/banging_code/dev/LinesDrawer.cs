using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace banging_code.dev
{
    public class LinesDrawer
    {
        private readonly List<LineRenderer> lines = new();
        private readonly Transform owner;

        public LinesDrawer(Transform owner)
        {
            this.owner = owner;
        }

        public int LinesCount => lines.Count;

        public void Draw(string name, Color color, float thikness, Vector3[] positions)
        {
            lines.Add(new GameObject(name, typeof(LineRenderer)).GetComponent<LineRenderer>());
            lines.Last().transform.parent = owner;
            lines.Last().sortingOrder = 10;
            lines.Last().positionCount = positions.Length;
            lines.Last().SetPositions(positions);
            lines.Last().startColor = color;
            lines.Last().endColor = color;
            lines.Last().startWidth = thikness;
            lines.Last().endWidth = thikness;
        }

        public void Clear()
        {
            foreach (var line in lines)
            {
                GameObject.Destroy(line.gameObject);
            }

            lines.Clear();
        }
    }
}