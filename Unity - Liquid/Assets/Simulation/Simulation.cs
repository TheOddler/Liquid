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

	[SerializeField]
	Brush _addingBrush;

	//
	// Schaders
	// ---
	[Header("Simulation Shaders")]
	[SerializeField]
	Shader _addSourceBrushShader;
	[SerializeField]
	Shader _updateOutflowFluxShader;
	[SerializeField]
	Shader _updateWaterHeightShader;
	[SerializeField]
	Shader _updateVelocityFieldShader;

	//
	// Materials
	// ---
	Material _addSourceBrushMaterial;
	Material _updateOutflowFluxMaterial;
	Material _updateWaterHeightMaterial;
	Material _updateVelocityFieldMaterial;

	//
	// Textures
	// ---
	BufferedRenderTexture _waterSandRock; // R: water, G: sand, B: rock
	public Texture CurrentWaterSandRock { get { return _waterSandRock.Texture; } }
	BufferedRenderTexture _outflowFluxRLBT; // outflowflux R: right, G: left, B: bottom, A: top
	public Texture CurrentOutflowFluxRLBT { get { return _outflowFluxRLBT.Texture; } }
	BufferedRenderTexture _velocityXY; //velocity: R: x, G: y
	public Texture CurrentVelocityXY { get { return _velocityXY.Texture; } }

	//
	// Other
	// ---
	float _lastUpdated;
	Collider _collider;

	//
	// Code
	// ---
	void Start ()
	{
		// Some assurances
		Assert.raiseExceptions = true;
		Assert.IsFalse(_initialWaterSandRock == null, "Missing initial water,sand,rock texture."); //IsNotNull doesn't work for some reason

		// Create materials
		_addSourceBrushMaterial = new Material(_addSourceBrushShader);
		_updateOutflowFluxMaterial = new Material(_updateOutflowFluxShader);
		_updateWaterHeightMaterial = new Material(_updateWaterHeightShader);
		//_updateVelocityFieldMaterial = new Material(_updateVelocityFieldShader);

		// Create textures
		var format = RenderTextureFormat.ARGBFloat;
		var readWrite = RenderTextureReadWrite.Linear;
		Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(format), "Rendertexture format not supported: " + format);
		_waterSandRock = new BufferedRenderTexture(_size, _size, 0, format, readWrite, _initialWaterSandRock);
		_outflowFluxRLBT = new BufferedRenderTexture(_size, _size, 0, format, readWrite, Texture2D.blackTexture);
		_velocityXY = new BufferedRenderTexture(_size, _size, 0, format, readWrite, Texture2D.blackTexture);

		// Some other variables
		_collider = GetComponent<Collider>();

		// Start first simulation step
		_lastUpdated = Time.time;
		UpdateSimulation();
	}
	
	void Update ()
	{
		bool addWater = Input.GetMouseButton(0);
		bool addSand = Input.GetMouseButton(1);
		if (addWater || addSand)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (_collider.Raycast(ray, out hitInfo, float.PositiveInfinity))
			{
				int halfSize = _addingBrushSize / 2;

				int x = Mathf.RoundToInt(hitInfo.textureCoord.x * _size);
				int y = Mathf.RoundToInt(hitInfo.textureCoord.y * _size);

				AddSource(_addingBrush, new Vector2(x, y));
			}
		}

		while (Time.time >= _lastUpdated + _updateInterval)
		{
			UpdateSimulation();
			_lastUpdated = Time.time;
		}
	}

	public void AddSource(Brush brush, Vector2 mid)
	{
		var currentActiveRT = RenderTexture.active;
		RenderTexture.active = _waterSandRock.Texture;

		mid -= brush.Size / 2;
		//mid.x = _waterSandRock.Texture.width - mid.x;
		mid.y = _waterSandRock.Texture.height - mid.y;
		Rect screenRect = new Rect(mid, brush.Size);

		_addSourceBrushMaterial.SetFloat("_Scale", 0.1f);
		Graphics.DrawTexture(screenRect, brush.Texture, _addSourceBrushMaterial);

		RenderTexture.active = currentActiveRT;
	}

	void UpdateSimulation()
	{
		// Do all steps
		UpdateFluxStep();
		UpdateHeightStep();

		// Finalize
		_waterSandRock.Swap();
		_outflowFluxRLBT.Swap();
		_velocityXY.Swap();
	}

	void UpdateFluxStep()
	{
		// Set values
		_updateOutflowFluxMaterial.SetTexture("_WaterSandRockTex", _waterSandRock.Texture);
		_updateOutflowFluxMaterial.SetFloat("_DT", _updateInterval);
		_updateOutflowFluxMaterial.SetFloat("_L", _pipeLength);
		_updateOutflowFluxMaterial.SetFloat("_A", _pipeCrossSectionArea);
		_updateOutflowFluxMaterial.SetFloat("_G", _gravityConstant);

		// Do the step
		Graphics.Blit(_outflowFluxRLBT.Texture, _outflowFluxRLBT.Buffer, _updateOutflowFluxMaterial);
	}

	void UpdateHeightStep()
	{
		// Set values
		_updateWaterHeightMaterial.SetTexture("_OutflowFluxRLBT", _outflowFluxRLBT.Buffer);
		_updateWaterHeightMaterial.SetFloat("_DT", _updateInterval);
		_updateWaterHeightMaterial.SetFloat("_L", _pipeLength);

		// Do the step
		Graphics.Blit(_waterSandRock.Texture, _waterSandRock.Buffer, _updateWaterHeightMaterial);
	}
}
