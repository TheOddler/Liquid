using UnityEngine;

public struct BufferedRenderTexture
{
	RenderTexture _texture;
	public RenderTexture Texture { get { return _texture; } }

	RenderTexture _buffer;
	public RenderTexture Buffer { get { return _buffer; } }

	public BufferedRenderTexture(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite, Texture initial = null, TextureWrapMode wrapMode = TextureWrapMode.Repeat, FilterMode filterMode = FilterMode.Bilinear)
	{
		_texture = new RenderTexture(width, height, depth, format, readWrite);
		_texture.wrapMode = wrapMode;
		_texture.filterMode = filterMode;

		_buffer = new RenderTexture(width, height, depth, format, readWrite);
		_buffer.wrapMode = wrapMode;
		_buffer.filterMode = filterMode;

		if (initial != null)
		{
			Graphics.Blit(initial, _texture);
			Graphics.Blit(initial, _buffer);
		}
	}

	public void Swap()
	{
		var temp = _texture;
		_texture = _buffer;
		_buffer = temp;
	}
}
