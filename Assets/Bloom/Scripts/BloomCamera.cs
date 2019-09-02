using UnityEngine;

namespace PostEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class BloomCamera : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] Shader shader = null;
        [SerializeField] Texture2D noise = null;

        [Header("Settings")]
        [Range(256, 1024)] public int resolution = 512;
        [Range(2, 8)] public int iterations = 8;
        [Range(0, 10)] public float intensity = 0.8f;
        [Range(0, 10)] public float threshold = 0.6f;
        [Range(0, 1)] public float softKnee = 0.7f;

        Bloom bloom;

        void OnDestroy ()
        {
            if (bloom != null)
                bloom.Dispose();
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
        {
            if (shader == null)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (bloom == null)
                bloom = new Bloom(shader);

            bloom.iterations = iterations;
            bloom.intensity = intensity;
            bloom.threshold = threshold;
            bloom.softKnee = softKnee;

            var res = RenderTextureUtils.GetScreenResolution(resolution);
            var bloomTarget = RenderTexture.GetTemporary(res.x, res.y, 0, Ext.argbHalf);
            bloomTarget.filterMode = FilterMode.Bilinear;
            bloomTarget.wrapMode = TextureWrapMode.Clamp;

            bloom.Apply(source, bloomTarget, res);
            bloom.Combine(source, destination, bloomTarget, noise);

            RenderTexture.ReleaseTemporary(bloomTarget);
        }
    }
}