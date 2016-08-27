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

	[Header("Sand Settings")]
	[SerializeField]
	float _sandBlurPerSecond = 10.0f;

	[Header("Erosion Settings")]
	float _sedimentCapacityConstant = 1;
	[SerializeField]
	float _dissolvingConstant = 0.2f;
	[SerializeField]
	float _depositionConstant = 0.1f;

	[Header("Initializaton")]
	[SerializeField]
	Texture _initialWaterSandRock;

	//
	// Schaders
	// ---
	[Header("Simulation Shaders")]
	[SerializeField]
	Shader _updateOutflowFluxShader;
	[SerializeField]
	Shader _updateHeightsShader;
	[SerializeField]
	Shader _updateVelocityFieldShader;
	[SerializeField]
	Shader _updateErosionDepositionShader;

	//
	// Materials
	// ---
	Material _updateOutflowFluxMaterial;
	Material _updateHeightsMaterial;
	Material _updateVelocityFieldMaterial;
	Material _updateErosionDepositionMaterial;

	//
	// Textures
	// ---
	BufferedRenderTexture _waterSandRockSediment; // R: water, G: sand, B: rock, A: sediment
	public RenderTexture CurrentWaterSandRockSediment { get { return _waterSandRockSediment.Texture; } }
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
		_updateOutflowFluxMaterial = new Material(_updateOutflowFluxShader);
		_updateHeightsMaterial = new Material(_updateHeightsShader);
		_updateVelocityFieldMaterial = new Material(_updateVelocityFieldShader);
		_updateErosionDepositionMaterial = new Material(_updateErosionDepositionShader);

		// Create textures
		var format = RenderTextureFormat.ARGBFloat;
		var readWrite = RenderTextureReadWrite.Linear;
		Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(format), "Rendertexture format not supported: " + format);
		_waterSandRockSediment = new BufferedRenderTexture(_gridPixelCount, _gridPixelCount, 0, format, readWrite, _initialWaterSandRock);
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

	void UpdateSimulation()
	{
		// Do all steps
		UpdateFluxStep();
		UpdateHeightsStep();
		UpdateVelocityXY();
		UpdateErosionDeposition();
	}

	void UpdateFluxStep()
	{
		// Set values
		_updateOutflowFluxMaterial.SetTexture("_WaterSandRockSedimentTex", _waterSandRockSediment.Texture);
		_updateOutflowFluxMaterial.SetFloat("_DT", _updateInterval);
		_updateOutflowFluxMaterial.SetFloat("_L", _gridPixelSize);
		_updateOutflowFluxMaterial.SetFloat("_A", _pipeCrossSectionArea);
		_updateOutflowFluxMaterial.SetFloat("_G", _gravityConstant);

		// Do the step
		Graphics.Blit(_outflowFluxRLBT.Texture, _outflowFluxRLBT.Buffer, _updateOutflowFluxMaterial);

		// Finalize
		_outflowFluxRLBT.Swap();
	}

	void UpdateHeightsStep()
	{
		// Set values
		_updateHeightsMaterial.SetTexture("_OutflowFluxRLBT", _outflowFluxRLBT.Texture);
		_updateHeightsMaterial.SetFloat("_DT", _updateInterval);
		_updateHeightsMaterial.SetFloat("_L", _gridPixelSize);
		_updateHeightsMaterial.SetFloat("_SandBlurPerSecond", _sandBlurPerSecond);

		// Do the step
		Graphics.Blit(_waterSandRockSediment.Texture, _waterSandRockSediment.Buffer, _updateHeightsMaterial);

		// Finalize
		_waterSandRockSediment.Swap();
	}

	void UpdateVelocityXY()
	{
		// Set values
		_updateVelocityFieldMaterial.SetTexture("_OutflowFluxRLBT", _outflowFluxRLBT.Texture);
		_updateVelocityFieldMaterial.SetTexture("_WaterSandRockSedimentTex", _waterSandRockSediment.Texture);
		_updateVelocityFieldMaterial.SetTexture("_PreviousWaterSandRockSedimentTex", _waterSandRockSediment.Buffer);
		_updateVelocityFieldMaterial.SetFloat("_L", _gridPixelSize);

		// Do the step
		Graphics.Blit(_velocityXY.Texture, _velocityXY.Buffer, _updateVelocityFieldMaterial);

		// Finalize
		_velocityXY.Swap();
	}

	void UpdateErosionDeposition()
	{
		// Set values
		_updateErosionDepositionMaterial.SetTexture("_VelocityXY", _velocityXY.Buffer);

		_updateErosionDepositionMaterial.SetFloat("_DT", _updateInterval);
		_updateErosionDepositionMaterial.SetFloat("_L", _gridPixelSize);

		_updateErosionDepositionMaterial.SetFloat("_Kc", _sedimentCapacityConstant);
		_updateErosionDepositionMaterial.SetFloat("_Ks", _dissolvingConstant);
		_updateErosionDepositionMaterial.SetFloat("_Kd", _depositionConstant);

		// Do the step
		Graphics.Blit(_waterSandRockSediment.Texture, _waterSandRockSediment.Buffer, _updateErosionDepositionMaterial);

		// Finalize
		_waterSandRockSediment.Swap();
	}

	void UpdateSedimentTransportation()
	{
		
	}
}
