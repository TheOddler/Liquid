using UnityEngine;
using UnityEngine.Assertions;

public class SimDebug : MonoBehaviour
{
	[SerializeField]
	Simulation _sim;

	//
	// Settings
	// ---
	[SerializeField]
	bool debug_showVolumeInfo = false;

	//
	// Cache
	// ---
	Texture2D _cashedVolumeTex = null; //for faster volume cheking
	int _currentStepCount;
	CountingDict<int> _stepCounts = new CountingDict<int>();

	void OnEnable ()
	{
		if (_cashedVolumeTex == null)
		{
			_cashedVolumeTex = new Texture2D(_sim.GridPixelCount, _sim.GridPixelCount, TextureFormat.RGBAFloat, false);
		}

		_sim.OnBeforeSimFrame += OnBeforeSimFrame;
		_sim.OnSimStep += OnSimStep;
		_sim.OnAfterSimFrame += OnAfterSimFrame;
	}

	void OnDisable()
	{
		_sim.OnBeforeSimFrame -= OnBeforeSimFrame;
		_sim.OnSimStep -= OnSimStep;
		_sim.OnAfterSimFrame -= OnAfterSimFrame;
	}

	void OnBeforeSimFrame()
	{
		_currentStepCount = 0;
	}

	void OnSimStep()
	{
		_currentStepCount++;
	}

	void OnAfterSimFrame()
	{
		_stepCounts.Increase(_currentStepCount);
	}

	void OnGUI()
	{
		Assert.IsTrue(_sim.CurrentWaterSandRockSediment.format == RenderTextureFormat.ARGBFloat);

		GUI.Label(new Rect(10, Screen.height - 50, 150, 20), "Sim steps per frame:");
		GUI.Label(new Rect(160, Screen.height - 50, 1000, 20), _stepCounts.ToString());
		
		debug_showVolumeInfo = GUI.Toggle(new Rect(10, Screen.height - 30, 150, 20), debug_showVolumeInfo, "Show volume info?");
		if (debug_showVolumeInfo && Event.current.type == EventType.Repaint)
		{
			var currentActiveRT = RenderTexture.active;
			RenderTexture.active = _sim.CurrentWaterSandRockSediment;

			_cashedVolumeTex.ReadPixels(new Rect(0, 0, _cashedVolumeTex.width, _cashedVolumeTex.height), 0, 0);
			_cashedVolumeTex.Apply();

			RenderTexture.active = currentActiveRT;

			/*var rawColors = _cashedVolumeTex.GetRawTextureData();
			float[] colFloats = new float[rawColors.Length / 4];
			System.Buffer.BlockCopy(rawColors, 0, colFloats, 0, rawColors.Length);

			Vector4 totalmagn = Vector4.zero;
			for (int i = 0; i < colFloats.Length; i += 4)
			{
				totalmagn.x += colFloats[i + 0];
				totalmagn.y += colFloats[i + 1];
				totalmagn.z += colFloats[i + 2];
				totalmagn.w += colFloats[i + 3];
			}*/

			var cols = _cashedVolumeTex.GetPixels();

			Vector4 totalmagn = Vector4.zero;
			for (int i = 0; i < cols.Length; i += 4)
			{
				totalmagn.x += cols[i + 0].r;
				totalmagn.y += cols[i + 1].g;
				totalmagn.z += cols[i + 2].b;
				totalmagn.w += cols[i + 3].a;
			}

		   totalmagn *= _sim.GridPixelSize * _sim.GridPixelSize;

			GUI.Label(new Rect(160, Screen.height - 30, 1000, 20),
				"Water: " + totalmagn.x.ToString("0") +
				"   Sand: " + totalmagn.y.ToString("0") +
				"   SaSe: " + (totalmagn.y + totalmagn.w).ToString("0") +
				"      -> Calculating this is slow, disable for better performance.");
		}
	}
}
