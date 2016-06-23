﻿using UnityEngine;
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
	float _pipeLength = 0.2f;
	[SerializeField]
	float _pipeCrossSectionArea = 1.0f;
	[SerializeField]
	float _gravityConstant = 9.81f;

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
	[SerializeField]
	Shader _updateOutflowFluxShader;
	[SerializeField]
	Shader _updateWaterHeightShader;
	[SerializeField]
	Shader _updateVelocityFieldShader;

	//
	// Materials
	// ---
	Material _addSourceMaterial;
	Material _updateOutflowFluxMaterial;
	Material _updateWaterHeightMaterial;
	Material _updateVelocityFieldMaterial;

	//
	// Textures
	// ---
	BufferedRenderTexture _waterSandRock; // R: water, G: sand, B: rock
	BufferedRenderTexture _outflowFluxRLBT; // outflowflux R: right, G: left, B: bottom, A: top
	BufferedRenderTexture _velocityXY; //velocity: R: x, G: y

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
		_updateOutflowFluxMaterial = new Material(_updateOutflowFluxShader);
		//_updateWaterHeightMaterial = new Material(_updateWaterHeightShader);
		//_updateVelocityFieldMaterial = new Material(_updateVelocityFieldShader);

		// Create textures
		var format = RenderTextureFormat.ARGBFloat;
		var readWrite = RenderTextureReadWrite.Linear;
		Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(format), "Rendertexture format not supported: " + format);
		_waterSandRock = new BufferedRenderTexture(_size, _size, 0, format, readWrite, _initialWaterSandRock);
		_outflowFluxRLBT = new BufferedRenderTexture(_size, _size, 0, format, readWrite, Texture2D.blackTexture);
		_velocityXY = new BufferedRenderTexture(_size, _size, 0, format, readWrite, Texture2D.blackTexture);

		_clearSourceWaterSandRock = TextureUtil.GetBlackTexture(_size, _size, TextureFormat.RGBAFloat);
		_sourceWaterSandRock = TextureUtil.GetBlackTexture(_size, _size, TextureFormat.RGBAFloat);

		// Some other variables
		_lastUpdated = Time.time;
		_visualsMaterial = GetComponent<Renderer>().material;
		UpdateSimulation();
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
		UpdateFluxStep();

		// Finalize
		_waterSandRock.Swap();
		_outflowFluxRLBT.Swap();
		_velocityXY.Swap();

		// Update visualization
		_visualsMaterial.SetTexture("_WaterSandRockTex", _waterSandRock.Texture);
		_visualsMaterial.SetTexture("_FluxTex", _outflowFluxRLBT.Texture);
	}

	void AddSourceStep()
	{
		// Set values
		_addSourceMaterial.SetTexture("_SourceTex", _sourceWaterSandRock);

		// Do the step
		Graphics.Blit(_waterSandRock.Texture, _waterSandRock.Buffer, _addSourceMaterial);

		// Clear source
		_sourceWaterSandRock.SetPixels32(_clearSourceWaterSandRock.GetPixels32());
	}

	void UpdateFluxStep()
	{
		// Set values
		_updateOutflowFluxMaterial.SetTexture("_WaterSandRockTex", _waterSandRock.Buffer);
		_updateOutflowFluxMaterial.SetFloat("_DT", _updateInterval);
		_updateOutflowFluxMaterial.SetFloat("_L", _pipeLength);
		_updateOutflowFluxMaterial.SetFloat("_A", _pipeCrossSectionArea);
		_updateOutflowFluxMaterial.SetFloat("_G", _gravityConstant);

		// Do the step
		Graphics.Blit(_outflowFluxRLBT.Texture, _outflowFluxRLBT.Buffer, _updateOutflowFluxMaterial);
	}
}
