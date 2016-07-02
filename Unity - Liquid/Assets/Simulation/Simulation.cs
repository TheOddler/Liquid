﻿using UnityEngine;
using UnityEngine.Assertions;

public class Simulation : MonoBehaviour
{
	//
	// Settings
	// ---
	[Header("Settings")]
	[SerializeField]
	int _gridPixelCount = 1024;

	[SerializeField]
	float _updateInterval = 0.2f; // also called deltaTime in the paper, denoted "_DT" in shaders

	[SerializeField]
	float _gridPixelSize = 0.1f; // also called pipe-length (l) in the paper, denoted "_L" in shaders
	public float GridPixelSize { get { return _gridPixelSize; } }
	[SerializeField]
	float _pipeCrossSectionArea = 1.0f;
	[SerializeField]
	float _gravityConstant = 9.81f;

	[SerializeField]
	float _sandBlurPerSecond = 10.0f;

	[SerializeField]
	Texture _initialWaterSandRock;

	//
	// Schaders
	// ---
	[Header("Simulation Shaders")]
	[SerializeField]
	Shader _addSourceBrushShader;
	[SerializeField]
	Shader _updateOutflowFluxShader;
	[SerializeField]
	Shader _updateHeightsShader;
	[SerializeField]
	Shader _updateVelocityFieldShader;

	//
	// Materials
	// ---
	Material _addSourceBrushMaterial;
	Material _updateOutflowFluxMaterial;
	Material _updateHeightsMaterial;
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
	float _nextUpdate;

	//
	// Debug
	// ---
	bool debug_showVolumeInfo = false;
	Texture2D debug_cashedVolumeTex = null; //for faster volume cheking
	CountingDict<int> debug_stepCounts = new CountingDict<int>();

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
		_updateHeightsMaterial = new Material(_updateHeightsShader);
		//_updateVelocityFieldMaterial = new Material(_updateVelocityFieldShader);

		// Create textures
		var format = RenderTextureFormat.ARGBFloat;
		var readWrite = RenderTextureReadWrite.Linear;
		Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(format), "Rendertexture format not supported: " + format);
		_waterSandRock = new BufferedRenderTexture(_gridPixelCount, _gridPixelCount, 0, format, readWrite, _initialWaterSandRock);
		_outflowFluxRLBT = new BufferedRenderTexture(_gridPixelCount, _gridPixelCount, 0, format, readWrite, Texture2D.blackTexture);
		_velocityXY = new BufferedRenderTexture(_gridPixelCount, _gridPixelCount, 0, format, readWrite, Texture2D.blackTexture);

		// Start first simulation step
		_nextUpdate = Time.time;
	}
	
	void Update ()
	{
		int iter = 0;
		while (Time.time >= _nextUpdate)
		{
			UpdateSimulation();
			_nextUpdate += _updateInterval;
			iter++;
		}
		debug_stepCounts.Increase(iter);
	}

	public void AddSource(Brush brush, Vector2 mid, Vector4 amount)
	{
		var currentActiveRT = RenderTexture.active;
		RenderTexture.active = _waterSandRock.Texture;

		var brushSize = brush.SizeAsV2;
		mid *= _gridPixelCount;
		mid -= brushSize / 2;
		Rect screenRect = new Rect(mid, brushSize);

		amount /= _gridPixelSize * _gridPixelSize;
		amount = Vector4.Scale(amount, brush.Scale); // scale so the brush's total volume is 1
		_addSourceBrushMaterial.SetVector("_Scale", amount);

		GL.PushMatrix();
		GL.LoadPixelMatrix(0, _gridPixelCount, _gridPixelCount, 0);
		Graphics.DrawTexture(screenRect, brush.Texture, _addSourceBrushMaterial);
		GL.PopMatrix();

		RenderTexture.active = currentActiveRT;
	}

	void UpdateSimulation()
	{
		// Do all steps
		UpdateFluxStep();
		UpdateHeightsStep();

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
		_updateOutflowFluxMaterial.SetFloat("_L", _gridPixelSize);
		_updateOutflowFluxMaterial.SetFloat("_A", _pipeCrossSectionArea);
		_updateOutflowFluxMaterial.SetFloat("_G", _gravityConstant);

		// Do the step
		Graphics.Blit(_outflowFluxRLBT.Texture, _outflowFluxRLBT.Buffer, _updateOutflowFluxMaterial);
	}

	void UpdateHeightsStep()
	{
		// Set values
		_updateHeightsMaterial.SetTexture("_OutflowFluxRLBT", _outflowFluxRLBT.Buffer);
		_updateHeightsMaterial.SetFloat("_DT", _updateInterval);
		_updateHeightsMaterial.SetFloat("_L", _gridPixelSize);
		_updateHeightsMaterial.SetFloat("_SandBlurPerSecond", _sandBlurPerSecond);

		// Do the step
		Graphics.Blit(_waterSandRock.Texture, _waterSandRock.Buffer, _updateHeightsMaterial);
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, Screen.height - 50, 150, 20), "Sim steps per frame:");
		GUI.Label(new Rect(160, Screen.height - 50, 1000, 20), debug_stepCounts.ToString());

		debug_showVolumeInfo = GUI.Toggle(new Rect(10, Screen.height - 30, 150, 20), debug_showVolumeInfo, "Show volume info?");
		if (debug_showVolumeInfo && Event.current.type == EventType.Repaint)
		{
			if (debug_cashedVolumeTex == null)
			{
				debug_cashedVolumeTex = new Texture2D(_gridPixelCount, _gridPixelCount, TextureFormat.RGBAFloat, false);
			}

			var currentActiveRT = RenderTexture.active;
			RenderTexture.active = _waterSandRock.Texture;

			debug_cashedVolumeTex.ReadPixels(new Rect(0, 0, debug_cashedVolumeTex.width, debug_cashedVolumeTex.height), 0, 0);
			debug_cashedVolumeTex.Apply();

			RenderTexture.active = currentActiveRT;

			var rawColors = debug_cashedVolumeTex.GetRawTextureData();
			float[] colFloats = new float[rawColors.Length / 4];
			System.Buffer.BlockCopy(rawColors, 0, colFloats, 0, rawColors.Length);

			Vector4 totalmagn = Vector4.zero;
			for (int i = 0; i < colFloats.Length; i += 4)
			{
				totalmagn.x += colFloats[i + 0];
				totalmagn.y += colFloats[i + 1];
				totalmagn.z += colFloats[i + 2];
				totalmagn.w += colFloats[i + 3];
			}

			totalmagn *= _gridPixelSize * _gridPixelSize;

			GUI.Label(new Rect(160, Screen.height - 30, 1000, 20),
				"Water: " + totalmagn.x.ToString("0") +
				"   Sand: " + totalmagn.y.ToString("0") +
				"      -> Calculating this is slow, disable for better performance.");
		}
	}
}
