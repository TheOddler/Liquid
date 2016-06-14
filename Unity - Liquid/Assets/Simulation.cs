using UnityEngine;
using System.Collections;

public class Simulation : MonoBehaviour
{
	public Texture _testInitialTexture;
	public RenderTexture _testTexture;
	private RenderTexture _testBuffer;
	public Material _testMaterial;

	public float _updateInterval = 1f;
	private float _lastUpdated;
	
	void Start ()
	{
		Graphics.Blit(_testInitialTexture, _testTexture);
		
		_testBuffer = new RenderTexture(
			_testTexture.width, _testTexture.height,
			_testTexture.depth, _testTexture.format);

		_lastUpdated = Time.time;
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
		Graphics.Blit(_testTexture, _testBuffer, _testMaterial);
		Graphics.Blit(_testBuffer, _testTexture);
	}
}
