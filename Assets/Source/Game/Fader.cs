using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(FaderRenderer), PostProcessEvent.BeforeStack, "Custom/Fader")]
public sealed class Fader : PostProcessEffectSettings
{
    #region Fields

    [Tooltip("Color effect intensity.")]
    public ColorParameter Color = new ColorParameter
    {
        value = UnityEngine.Color.white
    };

    #endregion
}

public sealed class FaderRenderer : PostProcessEffectRenderer<Fader>
{
    #region Methods

    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Custom/Fader"));
        sheet.properties.SetColor("_Color", settings.Color);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }

    #endregion
}