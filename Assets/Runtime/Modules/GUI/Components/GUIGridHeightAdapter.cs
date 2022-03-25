using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectX
{
    public class GUIGridHeightAdapter : MonoBehaviour
    {
        [SerializeField]
        private GridLayoutGroup m_GridLayout = null;

        public void Adapt()
        {
            RectTransform t = m_GridLayout.GetComponent<RectTransform>();
            float h = this.ComputeLayoutHeight(m_GridLayout);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        void Awake()
        {
            if (m_GridLayout == null)
            {
                m_GridLayout = this.GetComponent<GridLayoutGroup>();
            }
        }

        float ComputeLayoutHeight(GridLayoutGroup grid)
        {
            float cellH = grid.cellSize.y;
            float spaceH = grid.spacing.y;

            int cells = this.GetCellCount(grid);
            int xCount = grid.constraintCount;
            int yCount = cells % xCount != 0 ? cells / xCount + 1 : cells / xCount;

            float height = (cellH + spaceH) * yCount + spaceH;
            return height;
        }

        int GetCellCount(GridLayoutGroup grid)
        {
            int count = 0;
            Transform t = grid.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                if (t.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }
    }
}
