using UnityEngine;

// Attach this script to your Camera in the scene
[RequireComponent(typeof(Camera))]
public class Blur : MonoBehaviour
{
  // The material that has the blur shader
  // You can create a Shader that does a Gaussian blur, then create a Material from it.
  public Material blurMaterial;

  // Number of blur iterations
  // More iterations = smoother blur, but more expensive
  [Range(1, 4)]
  public int iterations = 2;

  // Downscale factor (for performance). For example, 2 means half resolution
  [Range(1, 4)]
  public int downscale = 2;

  // OnRenderImage is called after the scene is rendered to "src", 
  // and before it is output to the screen in "dest"
  private void OnRenderImage(RenderTexture src, RenderTexture dest)
  {
    if (blurMaterial == null)
    {
      // If no blur material, just send the image straight to dest (no blur).
      Graphics.Blit(src, dest);
      return;
    }

    // Temporary textures for blitting
    int width = src.width / downscale;
    int height = src.height / downscale;

    // Create two temporary RenderTextures for the blur passes
    RenderTexture temp1 = RenderTexture.GetTemporary(width, height, 0);
    RenderTexture temp2 = RenderTexture.GetTemporary(width, height, 0);

    // Downscale step
    Graphics.Blit(src, temp1);

    // Perform the blur for the set number of iterations
    for (int i = 0; i < iterations; i++)
    {
      // Horizontal blur
      blurMaterial.SetVector("_OffsetDir", new Vector2(1.0f / width, 0.0f));
      Graphics.Blit(temp1, temp2, blurMaterial);

      // Vertical blur
      blurMaterial.SetVector("_OffsetDir", new Vector2(0.0f, 1.0f / height));
      Graphics.Blit(temp2, temp1, blurMaterial);
    }

    // Copy the blurred result from temp1 to final screen output
    Graphics.Blit(temp1, dest);

    // Release temporary textures
    RenderTexture.ReleaseTemporary(temp1);
    RenderTexture.ReleaseTemporary(temp2);
  }
}
