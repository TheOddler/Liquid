using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshCollider))]
public class SimulationInteraction : MonoBehaviour
{
	[SerializeField]
	Simulation _sim;

	//
	// Settings
	// ---
	[SerializeField]
	float _addingSpeed = 100f;

	[SerializeField]
	Brush _addingBrush;

	[SerializeField]
	Shader _addSourceBrushShader;

	//
	// Cache
	// ---
	Material _addSourceBrushMaterial;
	Collider _collider;

	void Start()
	{
		_addSourceBrushMaterial = new Material(_addSourceBrushShader);
		_collider = GetComponent<Collider>();
	}

	void Update()
	{
		bool addWater = Input.GetMouseButton(0);
		bool addSand = Input.GetMouseButton(1);
		if (addWater || addSand)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
			{
				Vector2 hitPos = hitInfo.textureCoord;

				Vector4 amount = new Vector4(
					addWater ? _addingSpeed : 0f, // water
					addSand ? _addingSpeed : 0f, // sand
					0, 0);

				AddSource(_addingBrush, hitPos, amount * Time.deltaTime);
			}
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
			{
				Vector2 hitPos = hitInfo.textureCoord;

				Vector4 amount = new Vector4(20f, 20f, 0, 0);
				AddSource(_addingBrush, hitPos, amount);
			}
		}
	}

	public void AddSource(Brush brush, Vector2 mid, Vector4 amount)
	{
		var currentActiveRT = RenderTexture.active;
		RenderTexture.active = _sim.CurrentWaterSandRock;

		var brushSize = brush.SizeAsV2;
		mid *= _sim.GridPixelCount;
		mid -= brushSize / 2;
		Rect screenRect = new Rect(mid, brushSize);

		amount /= _sim.GridPixelSize * _sim.GridPixelSize;
		amount = Vector4.Scale(amount, brush.Scale); // scale so the brush's total volume is 1
		_addSourceBrushMaterial.SetVector("_Scale", amount);

		GL.PushMatrix();
		GL.LoadPixelMatrix(0, _sim.GridPixelCount, _sim.GridPixelCount, 0);
		Graphics.DrawTexture(screenRect, brush.Texture, _addSourceBrushMaterial);
		GL.PopMatrix();

		RenderTexture.active = currentActiveRT;
	}
}
