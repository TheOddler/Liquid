using UnityEngine;
using System.Collections;

public static class TextureUtil
{
	static public Texture GetBlackTexture(int width, int height, TextureFormat format, bool mipmap = false, bool linear = true)
	{
		var texture = new Texture2D(1, 1, format, mipmap, linear);
		texture.SetPixel(0, 0, Color.black);
		texture.Resize(width, height);
		return texture;
	}
}
