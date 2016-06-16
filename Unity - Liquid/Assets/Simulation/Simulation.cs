using UnityEngine;
using UnityEngine.Assertions;

public class Simulation : MonoBehaviour
{
	//
	// Settings
	// ---
	[Header("Settings")]
	[SerializeField]
	int _size = 1024;

	[SerializeField]
	float _updateInterval = 0.2f;

	[SerializeField]
	Texture _initialWaterSandRock;

	[SerializeField]
	float _addingSpeed = 0.2f;
	[SerializeField]
	int _addingBrushSize = 15;

	//
	// Schaders
	// ---
	[Header("Simulation Shaders")]
	[SerializeField]
	Shader _addSourceShader;

	//
	// Materials
	// ---
	Material _addSourceMaterial;

	//
	// Textures
	// ---
	BufferedRenderTexture _waterSandRock;

	Texture2D _clearSourceWaterSandRock;
	Texture2D _sourceWaterSandRock;

	//
	// Other
	// ---
	float _lastUpdated;
	Material _visualsMaterial;
	
	//
	// Code
	// ---
	void Start ()
	{
		// Some assurances
		Assert.raiseExceptions = true;
		Assert.IsFalse(_initialWaterSandRock == null, "Missing initial water,sand,rock texture."); //IsNotNull doesn't work for some reason

		// Create materials
		_addSourceMaterial = new Material(_addSourceShader);

		// Create textures
		var format = RenderTextureFormat.ARGBFloat;
		var readWrite = RenderTextureReadWrite.Linear;
		_waterSandRock = new BufferedRenderTexture(_size, _size, 0, format, readWrite);

		_clearSourceWaterSandRock = TextureUtil.GetBlackTexture(_size, _size, TextureFormat.RGBAFloat);
		_sourceWaterSandRock = TextureUtil.GetBlackTexture(_size, _size, TextureFormat.RGBAFloat);

		// Further initialization
		Graphics.Blit(_initialWaterSandRock, _waterSandRock.Texture);

		// Some other variables
		_lastUpdated = Time.time;
		_visualsMaterial = GetComponent<Renderer>().material;
	}
	
	void Update ()
	{
		bool addWater = Input.GetMouseButton(0);
		bool addSand = Input.GetMouseButton(1);
		if (addWater || addSand)
		{
			int halfSize = _addingBrushSize / 2;

			int x = Mathf.RoundToInt(Input.mousePosition.x) * _sourceWaterSandRock.width / Screen.width;
			int y = Mathf.RoundToInt(Input.mousePosition.y) * _sourceWaterSandRock.height / Screen.height;
			x = Mathf.Clamp(x, halfSize, _sourceWaterSandRock.width - halfSize - 1);
			y = Mathf.Clamp(y, halfSize, _sourceWaterSandRock.height - halfSize - 1);

			var color = _sourceWaterSandRock.GetPixel(x, y);
			if (addWater) color.r += _addingSpeed * Time.deltaTime; // water
			if (addSand) color.g += _addingSpeed * Time.deltaTime; // sand
			//color.b += 0; // rock

			Color32[] colors = new Color32[_addingBrushSize * _addingBrushSize];
			for (int i = 0; i < colors.Length; ++i)
				colors[i] = color;
			_sourceWaterSandRock.SetPixels32(x - halfSize, y - halfSize, _addingBrushSize, _addingBrushSize, colors);
			_sourceWaterSandRock.Apply();
		}

		if (Time.time >= _lastUpdated + _updateInterval)
		{
			UpdateSimulation();
			_lastUpdated = Time.time;
		}
	}

	void UpdateSimulation()
	{
		// Do all steps
		AddSourceStep();

		// Finalize
		_waterSandRock.Swap();

		// Update visualization
		_visualsMaterial.SetTexture("_WaterSandRock", _waterSandRock.Texture);
	}

	void AddSourceStep()
	{
		_addSourceMaterial.SetTexture("_SourceTex", _sourceWaterSandRock);
		Graphics.Blit(_waterSandRock.Texture, _waterSandRock.Buffer, _addSourceMaterial);
		_sourceWaterSandRock.SetPixels32(_clearSourceWaterSandRock.GetPixels32());
	}
}
