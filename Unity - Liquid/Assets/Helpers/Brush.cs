using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Brush: ScriptableObject
{
	[SerializeField]
	Texture2D _texture;
	public Texture2D Texture { get { return _texture; } }

	Vector4 _invTotalColor;
	public Vector4 Scale // 1 / total color value, per channel
	{
		get
		{
			return _invTotalColor * (_texture.width * _texture.height) / (_size.x * _size.y);
		}
	}

	[SerializeField]
	Vector2 _size;
	public Vector2 Size { get{ return _size; } }

	void OnEnable()
	{
		InitializeInverseTotalColor();
	}

	void InitializeInverseTotalColor()
	{
		_invTotalColor = Vector4.zero;
		var cols = _texture.GetPixels();
		for (int i = 0; i < cols.Length; ++i)
		{
			var col = cols[i];
			_invTotalColor.x += col.r;
			_invTotalColor.y += col.g;
			_invTotalColor.z += col.b;
			_invTotalColor.w += col.a;
		}

		_invTotalColor.x = 1.0f / _invTotalColor.x;
		_invTotalColor.y = 1.0f / _invTotalColor.y;
		_invTotalColor.z = 1.0f / _invTotalColor.z;
		_invTotalColor.w = 1.0f / _invTotalColor.w;
	}
}
