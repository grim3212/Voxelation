using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour {
	public GameObject mainMenuObject;
	public GameObject settingsObject;

	[Header ("Main Menu UI Elements")]
	public TextMeshProUGUI seedField;

	[Header ("Settings Menu UI Elements")]
	public Slider viewDistanceSlider;
	public TextMeshProUGUI viewDistanceText;
	public Slider mouseSlider;
	public TextMeshProUGUI mouseTextSlider;
	public Toggle threadingToggle;
	public Toggle chunkAnimationToggle;
    public TMP_Dropdown clouds;


	Settings settings;

	private void Awake () {
		if (!File.Exists (Application.dataPath + "/settings.cfg")) {
			Debug.Log ("Creating settings file");
			settings = new Settings ();

			string jsonExport = JsonUtility.ToJson (settings, true);
			File.WriteAllText (Application.dataPath + "/settings.cfg", jsonExport);
		}
		else {

			Debug.Log ("Settings file found");

			string jsonImport = File.ReadAllText (Application.dataPath + "/settings.cfg");
			settings = JsonUtility.FromJson<Settings> (jsonImport);

		}
	}

	public void StartGame () {
		VoxelData.seed = Mathf.Abs (seedField.text.GetHashCode () / VoxelData.WorldSizeInChunks);
		SceneManager.LoadScene ("Game", LoadSceneMode.Single);
	}

	public void EnterSettings () {
		viewDistanceSlider.value = settings.viewDistance;
		UpdateViewDistanceSlider ();

		mouseSlider.value = settings.mouseSensitivity;
		UpdateMouseSlider ();

		threadingToggle.isOn = settings.enableThreading;
		chunkAnimationToggle.isOn = settings.enableAnimatedChunks;

        clouds.value = (int)settings.clouds;

		mainMenuObject.SetActive (false);
		settingsObject.SetActive (true);
	}

	public void LeaveSettings () {
		settings.viewDistance = (int)viewDistanceSlider.value;
		settings.mouseSensitivity = mouseSlider.value;
		settings.enableThreading = threadingToggle.isOn;
		settings.enableAnimatedChunks = chunkAnimationToggle.isOn;
        settings.clouds = (CloudStyle)clouds.value;

        string jsonExport = JsonUtility.ToJson (settings, true);
		File.WriteAllText (Application.dataPath + "/settings.cfg", jsonExport);

		mainMenuObject.SetActive (true);
		settingsObject.SetActive (false);
	}

	public void QuitGame () {
		Application.Quit ();
	}

	public void UpdateViewDistanceSlider () {
		viewDistanceText.text = "View Distance : " + viewDistanceSlider.value;
	}
	public void UpdateMouseSlider () {
		mouseTextSlider.text = "Mouse Sensitivity : " + mouseSlider.value.ToString ("F2");
	}
}
