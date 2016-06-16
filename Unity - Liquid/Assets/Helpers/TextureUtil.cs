using UnityEngine;

public static class TextureUtil
{
	static public Texture2D GetBlackTexture(int width, int height, TextureFormat format, bool mipmap = false, bool linear = true)
	{
		var texture = new Texture2D(1, 1, format, mipmap, linear);
		texture.SetPixel(0, 0, Color.black);
		texture.Resize(width, height);
		return texture;
	}
}
