using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class Brush: ScriptableObject
{
	[SerializeField]
	Texture2D _texture;
	public Texture2D Texture { get { return _texture; } }

	public float TotalColor
	{
		get
		{
			return 1.0f;
		}
	}

	public Vector2 Size 
	{ 
		get
		{ 
			return new Vector2(_texture.width, _texture.height); 
		}
	}
}
