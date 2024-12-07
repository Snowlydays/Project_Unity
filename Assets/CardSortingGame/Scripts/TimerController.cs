using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject readyButton; // 準備完了ボタン
    LineRenderer circleLine;

    public float radius = 1f;
    public int segments = 64;
    public float width = 10f;
    public float middlex=0f;
    public float middley=0f;

    void Start()
    {
        circleLine=readyButton.GetComponent<LineRenderer>();
        circleLine.material = new Material(Shader.Find("Sprites/Default"));
        circleLine.loop = true;
        circleLine.useWorldSpace = false; // local space
    }

    void FixedUpdate()
    {
        circleLine.widthMultiplier = width;
        circleLine.positionCount = segments + 1;

        float deltaTheta = (2f * Mathf.PI) / segments;
        float theta = 0f;

        for (int i = 0; i < segments + 1; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, y, -5f);
            circleLine.SetPosition(i, pos);
            theta += deltaTheta;
        }
        circleLine.startColor = Color.white;
        circleLine.endColor = Color.white;
    }
}
