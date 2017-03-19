using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    private Renderer[] m_renderers;
    private float[] m_speeds;

    private float m_waveTime;

	private void Awake ()
    {
        m_renderers = GetComponentsInChildren<Renderer>();
        m_speeds = new float[m_renderers.Length];
        for (int i = 0; i < m_renderers.Length; i++)
        {
            m_speeds[i] = Random.Range(0.5f, 1.5f);
        }
    }
	
	private void Update ()
    {
        m_waveTime += Time.deltaTime / 10;

        for (int i = 0; i < m_renderers.Length; i++)
        {
            m_renderers[i].material.SetTextureOffset("_MainTex", new Vector2(m_waveTime * m_speeds[i], 0));
        }
	}
}
