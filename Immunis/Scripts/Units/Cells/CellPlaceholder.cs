using UnityEngine;
using System.Collections;

public class CellPlaceholder : MonoBehaviour
{
    //static private float s_placeholderCellScaling = 4;

    private Cell m_cell;
    private CellRenderer m_cellRenderer;

    private Vector3 m_rendererLocalPosition;
    private Quaternion m_rendererLocalRotation;
    private Vector3 m_rendererLocalScale;

    private void TransformCellRenderer()
    {
        m_cellRenderer.transform.SetParent(transform);

        m_cellRenderer.transform.localPosition = m_rendererLocalPosition;
        m_cellRenderer.transform.localRotation = m_rendererLocalRotation;
        m_cellRenderer.transform.localScale = m_rendererLocalScale;
    }

    public void SetCell(Cell cell)
    {
        if(cell == m_cell)
        {
            return;
        }

        if(m_cellRenderer != null)
        {
            Destroy(m_cellRenderer.gameObject);
        }

        m_cell = cell;

        m_cellRenderer = Instantiate(m_cell.cellRendererPrefab);

        m_rendererLocalPosition = m_cellRenderer.transform.position;
        m_rendererLocalRotation = m_cellRenderer.transform.rotation;
        m_rendererLocalScale = m_cellRenderer.transform.lossyScale;

        TransformCellRenderer();
    }

    public void ReclaimCellRenderer()
    {
        m_cellRenderer.gameObject.SetActive(true);
        TransformCellRenderer();
        m_cellRenderer.SetCellRenderingMode(CellRenderingMode.Default);
        m_cellRenderer.decorum.Revive();
    }

    public Cell cell
    {
        get { return m_cell; }
    }

    public CellRenderer cellRenderer
    {
        get { return m_cellRenderer; }
    }
}
