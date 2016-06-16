using UnityEngine;

public struct BufferedRenderTexture
{
	RenderTexture _texture;
	public RenderTexture Texture { get { return _texture; } }

	RenderTexture _buffer;
	public RenderTexture Buffer { get { return _buffer; } }

	public BufferedRenderTexture(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		_texture = new RenderTexture(width, height, depth, format, readWrite);
		_buffer = new RenderTexture(width, height, depth, format, readWrite);
	}

	public void Swap()
	{
		var temp = _texture;
		_texture = _buffer;
		_buffer = temp;
	}
}
