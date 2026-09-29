using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SAFEX glowing evacuation route.
/// Uses a LineRenderer so the route remains lightweight on mobile AR.
/// </summary>
public class SAFEXRouteVisualizer : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private float lineWidth = 0.045f;
    [SerializeField] private float yOffset = 0.025f;

    [Header("Animation")]
    [SerializeField] private bool animate = true;
    [SerializeField] private float pulseSpeed = 2.5f;

    private LineRenderer line;
    private Material routeMaterial;
    private Color baseColor = new Color(0.05f, 1f, 0.35f, 1f);

    private void Awake()
    {
        line = gameObject.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 0;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.numCapVertices = 4;
        line.numCornerVertices = 4;

        Shader shader =
            Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        routeMaterial = new Material(shader);
        routeMaterial.color = baseColor;

        line.material = routeMaterial;
    }

    public void SetRoute(
        IList<Vector3> worldPoints)
    {
        if (worldPoints == null || worldPoints.Count < 2)
        {
            ClearRoute();
            return;
        }

        line.positionCount = worldPoints.Count;

        for (int i = 0; i < worldPoints.Count; i++)
        {
            Vector3 point = worldPoints[i];
            point.y += yOffset;
            line.SetPosition(i, point);
        }

        line.enabled = true;
    }

    public void SetVisible(bool visible)
    {
        if (line != null)
            line.enabled = visible;
    }

    public void ClearRoute()
    {
        if (line != null)
        {
            line.positionCount = 0;
            line.enabled = false;
        }
    }

    private void Update()
    {
        if (!animate || routeMaterial == null)
            return;

        float pulse =
            0.65f +
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.17f;

        routeMaterial.color =
            new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                pulse
            );
    }

    private void OnDestroy()
    {
        if (routeMaterial != null)
            Destroy(routeMaterial);
    }
}
