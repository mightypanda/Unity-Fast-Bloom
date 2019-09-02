using System.Collections.Generic;
using UnityEngine;

namespace PostEffects
{
    public class Bloom
    {
        public Bloom (Shader shader)
        {
            this.shader = shader;
        }

        int mIterations = 8;
        public int iterations
        {
            get { return mIterations; }
            set
            {
                if (value != mIterations)
                    needResize = true;
                mIterations = value;
            }
        }

        public float intensity = 0.8f;
        public float threshold = 0.6f;
        public float softKnee = 0.7f;

        bool inited = false;

        Material material;
        Shader shader;
        List<RenderTexture> buffers = new List<RenderTexture>();
        Vector2Int currentResolution;
        bool needResize = true;

        int _Threshold = Shader.PropertyToID("_Threshold");
        int _Curve = Shader.PropertyToID("_Curve");
        int _TexelSize = Shader.PropertyToID("_TexelSize");
        int _Intensity = Shader.PropertyToID("_Intensity");
        int _SourceTex = Shader.PropertyToID("_SourceTex");
        int _NoiseTex = Shader.PropertyToID("_NoiseTex");
        int _NoiseTexScale = Shader.PropertyToID("_NoiseTexScale");

        const int PREFILTER = 0;
        const int DOWNSAMPLE = 1;
        const int UPSAMPLE = 2;
        const int FINAL = 3;
        const int COMBINE = 4;

        public void Dispose ()
        {
            foreach (var rt in buffers)
            {
                rt.Release();
                DestroyFunc(rt);
            }
            DestroyFunc(material);
        }

        void Init ()
        {
            if (inited) return;
            if (shader == null) return;
            inited = true;
            material = CreateMaterial(shader);
            shader = null;
        }

        public void Apply (RenderTexture source, RenderTexture destination, Vector2Int resolution)
        {
            Init();
            if (!inited) return;

            if (currentResolution != resolution)
            {
                currentResolution = resolution;
                needResize = true;
            }

            if (needResize)
            {
                needResize = false;
                Resize(destination);
            }

            if (buffers.Count < 2) return;

            material.SetFloat(_Threshold, threshold);
            var knee = Mathf.Max(threshold * softKnee, 0.0001f);
            var curve = new Vector3(threshold - knee, knee * 2, 0.25f / knee);
            material.SetVector(_Curve, curve);

            var last = destination;
            Graphics.Blit(source, last, material, PREFILTER);

            foreach (var dest in buffers)
            {
                material.SetVector(_TexelSize, last.texelSize);
                Graphics.Blit(last, dest, material, DOWNSAMPLE);
                last = dest;
            }

            for (int i = buffers.Count - 2; i >= 0; i--)
            {
                var dest = buffers[i];
                material.SetVector(_TexelSize, last.texelSize);
                Graphics.Blit(last, dest, material, UPSAMPLE);
                last.DiscardContents();
                last = dest;
            }

            material.SetFloat(_Intensity, intensity);
            material.SetVector(_TexelSize, last.texelSize);
            Graphics.Blit(last, destination, material, FINAL);
            last.DiscardContents();
        }

        public void Combine (RenderTexture source, RenderTexture destination, RenderTexture bloom, Texture2D noise)
        {
            material.SetTexture(_SourceTex, source);
            material.SetTexture(_NoiseTex, noise);
            material.SetVector(_NoiseTexScale, RenderTextureUtils.GetTextureScreenScale(noise));
            Graphics.Blit(bloom, destination, material, COMBINE);
        }

        void Resize (RenderTexture target)
        {
            foreach (var rt in buffers)
            {
                rt.Release();
                DestroyFunc(rt);
            }
            buffers.Clear();

            for (int i = 0; i < iterations; i++)
            {
                int w = target.width >> (i + 1);
                int h = target.height >> (i + 1);

                if (w < 2 || h < 2) break;

                var rt = new RenderTexture(w, h, 0, target.format);
                rt.filterMode = FilterMode.Bilinear;
                rt.wrapMode = target.wrapMode;
                buffers.Add(rt);
            }
        }

        void DestroyFunc (Object obj)
        {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }

        Material CreateMaterial (Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.HideAndDontSave;
            return material;
        }
    }
}