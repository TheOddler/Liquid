using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Brush: ScriptableObject
{
	[SerializeField]
	Texture2D _texture;
	public Texture2D Texture { get { return _texture; } }
	
	Vector4 _inverseTotalMagnitude;
	public Vector4 Scale 
	{ 
		get 
		{
			
			return _inverseTotalMagnitude * ( (float)(_texture.width * _texture.height) / (float)(_size * _size) );
		}
	}

	[SerializeField]
	int _size = 10;
	public Vector2 SizeAsV2 { get { return new Vector2(_size, _size); } }

	void OnEnable()
	{
		Initialize();
	}

	void Initialize()
	{
		var totMagn = Vector4.zero;
		var cols = _texture.GetPixels();
		for (int i = 0; i < cols.Length; ++i)
		{
			var col = cols[i];
			totMagn.x += col.r;
			totMagn.y += col.g;
			totMagn.z += col.b;
			totMagn.w += col.a;
		}

		_inverseTotalMagnitude.x = 1 / totMagn.x;
		_inverseTotalMagnitude.y = 1 / totMagn.y;
		_inverseTotalMagnitude.z = 1 / totMagn.z;
		_inverseTotalMagnitude.w = 1 / totMagn.w;


		Debug.Log("Init: " + totMagn);
	}
}
