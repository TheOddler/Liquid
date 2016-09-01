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
	MeshCollider _collider;
	Texture2D _cashedHeightsTex = null;
	[SerializeField]
	SimVisManager _visManager;

	void Start()
	{
		_addSourceBrushMaterial = new Material(_addSourceBrushShader);
		_collider = GetComponent<MeshCollider>();
		
		_cashedHeightsTex = new Texture2D(_sim.GridPixelCount, _sim.GridPixelCount, TextureFormat.RGBAFloat, false); //smaller?
		_cashedHeightsTex.SetPixel(0, 0, Color.red);

		//StartCoroutine(UpdateCollider());
	}

	void Update()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
		{
			Vector2 hitPos = hitInfo.textureCoord;

			// Adding
			bool addWater = Input.GetMouseButton(0);
			bool addSand = Input.GetMouseButton(1);
			Vector4 amount = new Vector4(
				addWater ? _addingSpeed : 0f, // water
				addSand ? _addingSpeed : 0f, // sand
				0, 0);

			AddSource(_addingBrush, hitPos, amount * Time.deltaTime);

			// Visualize
			if (_visManager != null)
			{
				Vector2 fixedPos = hitPos;
				//fixedPos.x = 1 - fixedPos.x;
				fixedPos.y = 1 - fixedPos.y;
				_visManager.UpdateIndicator(fixedPos);
			}
		}
	}

	public void AddSource(Brush brush, Vector2 mid, Vector4 amount)
	{
		var currentActiveRT = RenderTexture.active;
		RenderTexture.active = _sim.CurrentWaterSandRockSediment;

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

	IEnumerator UpdateCollider()
	{
		yield return new WaitForSeconds(1);

		while (true)
		{
			// Get height data
			yield return new WaitForEndOfFrame();
			var currentActiveRT = RenderTexture.active;
			RenderTexture.active = _sim.CurrentWaterSandRockSediment;

			_cashedHeightsTex.ReadPixels(new Rect(0, 0, _cashedHeightsTex.width, _cashedHeightsTex.height), 0, 0, false);
			_cashedHeightsTex.Apply();

			RenderTexture.active = currentActiveRT;

			yield return new WaitForSeconds(1);

			var rawColors = _cashedHeightsTex.GetRawTextureData();
			float[] colFloats = new float[rawColors.Length / 4];
			System.Buffer.BlockCopy(rawColors, 0, colFloats, 0, rawColors.Length);

			/*Vector4 totalmagn = Vector4.zero;
			for (int i = 0; i < colFloats.Length; i += 4)
			{
				totalmagn.x += colFloats[i + 0];
				totalmagn.y += colFloats[i + 1];
				totalmagn.z += colFloats[i + 2];
				totalmagn.w += colFloats[i + 3];
			}*/

			yield return new WaitForSeconds(1);

			// Get mesh data
			Mesh mesh = _collider.sharedMesh;
			Vector3[] verts = mesh.vertices;
			Vector2[] uvs = mesh.uv;

			// Update the collider
			for (int i = 0; i < verts.Length; ++i)
			{
				Vector2 uv = uvs[i];
				Vector3 vert = verts[i];

				int x = Mathf.RoundToInt(uv.x * _sim.GridPixelCount);
				x = Mathf.Clamp(x, 0, _sim.GridPixelCount - 1);
				int y = Mathf.RoundToInt((1 - uv.y) * _sim.GridPixelCount);
				y = Mathf.Clamp(y, 0, _sim.GridPixelCount - 1);

				int infoPos = x * 4 + y * 4 * _sim.GridPixelCount;
				vert.y = colFloats[infoPos + 1]; //+1 voor sand

				verts[i] = vert;
			}
			mesh.vertices = verts;
			_collider.sharedMesh = mesh;

			yield return new WaitForSeconds(1);
		}
	}
}
