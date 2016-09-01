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
		_material.SetTexture("_WaterSandRockSediment", _sim.CurrentWaterSandRockSediment);
		_material.SetTexture("_Flux", _sim.CurrentOutflowFluxRLBT);
		_material.SetTexture("_VelocityXY", _sim.CurrentVelocityXY);
		_material.SetFloat("_DT", _sim.UpdateInterval);
		_material.SetFloat("_L", _sim.GridPixelSize);

		// Debug only?
		_material.SetFloat("_Kc", _sim.SedimentCapacityConstant);
		_material.SetFloat("_ErosionMinimumAngleThresshold", _sim.ErosionMinimumAngleThresshold);
	}

	public void UpdateIndicator(Vector2 pos)
	{
		_material.SetVector("_Indicator", new Vector4(pos.x, pos.y, 0, 0));
	}
}
