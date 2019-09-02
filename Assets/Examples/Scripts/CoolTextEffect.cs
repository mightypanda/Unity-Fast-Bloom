using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTextEffect : MonoBehaviour
{
    struct TextData
    {
        public float hue;
        public float angle;
        public float targetAngle;
        public float angleVelocity;
        public float hueOffset;
        public float angleOffset;
    }

    [SerializeField] Text textPrefab = null;
    [SerializeField] Transform grid = null;

    List<Text> texts = new List<Text>();
    List<TextData> textsData = new List<TextData>();

    float timer;

    public void Awake ()
    {
        DestroyChildrens(grid);
        int length = 5;
        for (int i = 0; i < length; i++)
        {
            var text = Instantiate(textPrefab, grid, false);
            var scale = (i + 1) * 0.5f;
            text.transform.localScale = Vector2.one * scale;
            texts.Add(text);

            var data = new TextData();
            data.hueOffset = (float)i / (float)(length - 1) * 0.5f;
            data.angleOffset = (float)i / (float)(length - 1) * 90;
            textsData.Add(data);
        }
    }

    void Update ()
    {
        var dt = Time.deltaTime;
        UpdateData(dt);
        UpdateView();
    }

    void UpdateData (float dt)
    {
        for (int i = 0; i < textsData.Count; i++)
        {
            var data = textsData[i];

            data.hue += dt * 0.5f;
            data.hue = Mathf.Repeat(data.hue, 1);

            data.targetAngle += dt * 0.2f;
            data.targetAngle = Mathf.Repeat(data.targetAngle, 1);
            var targetAngle = data.targetAngle * 360;
            targetAngle = Mathf.Repeat(targetAngle + data.angleOffset, 360);

            data.angleVelocity = Spring(data.angle, targetAngle, data.angleVelocity, 5, dt);
            data.angle += data.angleVelocity * dt;
            data.angle = Mathf.Repeat(data.angle, 360);

            textsData[i] = data;
        }
    }

    void UpdateView ()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            var text = texts[i];
            var data = textsData[i];
            var hue = Mathf.Repeat(data.hue + data.hueOffset, 1);
            text.color = Color.HSVToRGB(hue, 1, 1);
            text.transform.localRotation = Quaternion.Euler(0, 0, data.angle);
        }
    }

    void DestroyChildrens (Transform transform)
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            var child = transform.GetChild(i).gameObject;
            Object.Destroy(child);
        }
    }

    float Spring (float value, float target, float velocity, float omega, float dt)
    {
        var n1 = velocity - (value - target) * (omega * omega * dt);
        var n2 = 1 + omega * dt;
        velocity = n1 / (n2 * n2);
        return velocity;
    }
}
