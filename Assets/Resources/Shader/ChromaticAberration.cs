using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

[Serializable, VolumeComponentMenu("Post-processing/Custom/ChromaticAberration")]
public sealed class ChromaticAberration : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("エフェクトの強さを変更する。")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 1f);

    Material m_Material;

    public bool IsActive() => m_Material != null && intensity.value > 0f;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    public override void Setup()
    {
        Shader shader = Shader.Find("Hidden/Shader/ChromaticAberration");
        if (shader != null)
            m_Material = new Material(shader);
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        m_Material.SetFloat("_Intensity", intensity.value);
        if (source is Texture2DArray)
        {
            m_Material.SetTexture("_MainTex", source);
        }
        else
        {
            Debug.Log("Expected a 2DArray texture but got a different type.");
        }

        HDUtils.DrawFullScreen(cmd, m_Material, destination);
    }

    public override void Cleanup() => CoreUtils.Destroy(m_Material);
}
