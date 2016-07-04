using UnityEngine;
using UnityEngine.Assertions;

public class Simulation : MonoBehaviour
{
	//
	// Settings
	// ---
	[Header("Settings")]
	[SerializeField]
	int _gridPixelCount = 1024;
	public int GridPixelCount { get { return _gridPixelCount; } }

	[SerializeField]
	float _updateInterval = 0.005f; // also called deltaTime in the paper, denoted "_DT" in shaders

	[SerializeField]
	float _gridPixelSize = 0.1f; // also called pipe-length (l) in the paper, denoted "_L" in shaders
	public float GridPixelSize { get { return _gridPixelSize; } }
	[SerializeField]
	float _pipeCrossSectionArea = 0.1f;
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
	public RenderTexture CurrentWaterSandRock { get { return _waterSandRock.Texture; } }
	BufferedRenderTexture _outflowFluxRLBT; // outflowflux R: right, G: left, B: bottom, A: top
	public RenderTexture CurrentOutflowFluxRLBT { get { return _outflowFluxRLBT.Texture; } }
	BufferedRenderTexture _velocityXY; //velocity: R: x, G: y
	public RenderTexture CurrentVelocityXY { get { return _velocityXY.Texture; } }

	//
	// Other
	// ---
	float _nextUpdate;

	//
	// Events
	// ---
	public event System.Action OnBeforeSimFrame;
	public event System.Action OnSimStep;
	public event System.Action OnAfterSimFrame;

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
		if (OnBeforeSimFrame != null) OnBeforeSimFrame();
		while (Time.time >= _nextUpdate)
		{
			UpdateSimulation();
			_nextUpdate += _updateInterval;
			iter++;
			if (OnSimStep != null) OnSimStep();
		}
		if (OnAfterSimFrame != null) OnAfterSimFrame();
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
}
