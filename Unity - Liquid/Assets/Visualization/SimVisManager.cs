using UnityEngine;
using System.Collections;

public class SimVisManager : MonoBehaviour
{
	Simulation _sim;

	Material _material;

	void Start()
	{
		// Simulation
		_sim = GetComponentInParent<Simulation>();
		
		// Material
		var firstRenderer = GetComponentInChildren<Renderer>();
		_material = new Material(firstRenderer.sharedMaterial);

		foreach (var rend in GetComponentsInChildren<Renderer>())
		{
			rend.sharedMaterial = _material;
		}
	}

	void Update()
	{
		// Update visualization
		_material.SetTexture("_WaterSandRockTex", _sim.CurrentWaterSandRock);
		_material.SetTexture("_FluxTex", _sim.CurrentOutflowFluxRLBT);
		_material.SetTexture("_VelocityTex", _sim.CurrentVelocityXY);
		_material.SetFloat("_L", _sim.GridPixelSize);
	}
}
