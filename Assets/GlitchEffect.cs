/**
This work is licensed under a Creative Commons Attribution 3.0 Unported License.
http://creativecommons.org/licenses/by/3.0/deed.en_GB

You are free:

to copy, distribute, display, and perform the work
to make derivative works
to make commercial use of the work
*/

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
[UnityEngine.Rendering.PostProcessing.PostProcess(typeof(GlitchRenderer), PostProcessEvent.AfterStack, "Custom/Glitch")]
public sealed class Glitch : PostProcessEffectSettings
{
    [Range(0, 1)]
    public FloatParameter intensity = new FloatParameter();

    [Range(0, 1)]
    public FloatParameter flipIntensity = new FloatParameter();

    [Range(0, 1)]
    public FloatParameter colorIntensity = new FloatParameter();


}

public sealed class GlitchRenderer : PostProcessEffectRenderer<Glitch> {
    public Shader shader;
    public Texture2D displacementMap;
    float glitchup, glitchdown, flicker,
            glitchupTime = 0.05f, glitchdownTime = 0.05f, flickerTime = 0.5f;

    public override void Init()
    {
        shader = Shader.Find("Hidden/GlitchShader");
        base.Init();
    }

    // Called by camera to apply image effect
    public override void Render (PostProcessRenderContext context) {
        Material material = new Material(shader);
		material.SetFloat("_Intensity", settings.intensity);
        material.SetFloat("_ColorIntensity", settings.colorIntensity);
		material.SetTexture("_DispTex", displacementMap);
        
        flicker += Time.unscaledDeltaTime * settings.colorIntensity;
        if (flicker > flickerTime){
			material.SetFloat("filterRadius", Random.Range(-3f, 3f) * settings.colorIntensity);
            material.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * settings.colorIntensity, Vector3.forward) * Vector4.one);
            flicker = 0;
			flickerTime = Random.value;
		}

        if (settings.colorIntensity == 0)
            material.SetFloat("filterRadius", 0);
        
        glitchup += Time.unscaledDeltaTime * settings.flipIntensity;
        if (glitchup > glitchupTime){
			if(Random.value < 0.1f * settings.flipIntensity)
				material.SetFloat("flip_up", Random.Range(0, 1f) * settings.flipIntensity);
			else
				material.SetFloat("flip_up", 0);
			
			glitchup = 0;
			glitchupTime = Random.value/10f;
		}

        if (settings.flipIntensity == 0)
            material.SetFloat("flip_up", 0);


        glitchdown += Time.unscaledDeltaTime * settings.flipIntensity;
        if (glitchdown > glitchdownTime){
			if(Random.value < 0.1f * settings.flipIntensity)
				material.SetFloat("flip_down", 1 - Random.Range(0, 1f) * settings.flipIntensity);
			else
				material.SetFloat("flip_down", 1);
			
			glitchdown = 0;
			glitchdownTime = Random.value/10f;
		}

        if (settings.flipIntensity == 0)
            material.SetFloat("flip_down", 1);

        if (Random.value < 0.05 * settings.intensity){
			material.SetFloat("displace", Random.value * settings.intensity);
			material.SetFloat("scale", 1 - Random.value * settings.intensity);
        }
        else
			material.SetFloat("displace", 0);
        context.command.Blit(context.source, context.destination, material);
	}
}
