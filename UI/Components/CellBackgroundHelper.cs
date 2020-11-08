using HMUI;
using UnityEngine;

namespace ServerBrowser.UI.Components
{
    public class CellBackgroundHelper : MonoBehaviour
    {
        public SelectableCell Cell;
        public ImageView Background;

        public void OnEnable()
        {
            Cell.highlightDidChangeEvent += OnHighlightDidChange;
            Cell.selectionDidChangeEvent += OnSelectionDidChange;

            UpdateCellBackground();
        }

        public void OnDisable()
        {
            Cell.highlightDidChangeEvent -= OnHighlightDidChange;
            Cell.selectionDidChangeEvent -= OnSelectionDidChange;
        }

        private void OnSelectionDidChange(SelectableCell cell, SelectableCell.TransitionType transition, object _)
        {
            UpdateCellBackground();
        }

        private void OnHighlightDidChange(SelectableCell cell, SelectableCell.TransitionType transition)
        {
            UpdateCellBackground();
        }

        private void UpdateCellBackground()
        {
            Background.color = new Color(192f / 255f, 43f / 255f, 180f / 255f, 1.0f);

            if (Cell.selected)
            {
                Background.color0 = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                Background.color1 = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                Background.enabled = true;
            }
            else if (Cell.highlighted)
            {
                Background.color0 = new Color(1.0f, 1.0f, 1.0f, 0.75f);
                Background.color1 = new Color(1.0f, 1.0f, 1.0f, 0.25f);
                Background.enabled = true;
            }
            else
            {
                Background.enabled = false;
            }
        }
    }
}
