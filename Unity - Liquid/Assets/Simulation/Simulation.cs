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

	Texture _sourceWaterSandRock;

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

		_sourceWaterSandRock = TextureUtil.GetBlackTexture(_size, _size, TextureFormat.RGBAFloat);

		// Further initialization
		Graphics.Blit(_initialWaterSandRock, _waterSandRock.Texture);

		// Some other variables
		_lastUpdated = Time.time;
		_visualsMaterial = GetComponent<Renderer>().material;
	}
	
	void Update ()
	{
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
	}
}
