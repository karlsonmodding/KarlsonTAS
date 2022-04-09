using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity;
using Harmony;
using TMPro;

[assembly: MelonInfo(typeof(Main),"TasMod" , "0.1.0", "Mang432")]
[assembly: MelonGame("Dani", "Karlson")]
class Main : MelonMod
{
	public static byte gameSpeed = 100;
	const float version = 1f;
	static MelonPreferences_Category category;
	static string savHotkey, loadHotkey;
	static MelonPreferences_Entry savPref, loadPref;
	static GameObject bulletPrefab;
	static Bullet[] allBullets;
	static GameObject[] allMovables;
	static Enemy[] allEnemies;
	public override void OnApplicationStart() {
		base.OnApplicationStart();
		Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\savestates");
		//MelonLogger.Msg(VersionToString(version));
		category = MelonPreferences.CreateCategory("TasMod");
		savPref = category.CreateEntry<char>("SavestateHotkey", 'r');
		loadPref = category.CreateEntry<char>("LoadstateHotkey", 't');
		savHotkey = savPref.GetValueAsString();
		loadHotkey = loadPref.GetValueAsString();
		//MelonPreferences.Save(); //crashes the mod, for unknown reasons to me
	}

	public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
		base.OnSceneWasInitialized(buildIndex, sceneName);
		if (buildIndex > 1)
		{
			MelonCoroutines.Start(InitDebug());
			if (bulletPrefab == null)
            {
				bulletPrefab = Object.FindObjectOfType<RangedWeapon>().projectile;
            }
			SetObjArray();
		}
		if (buildIndex == 1)
		{
			Options o = Object.FindObjectOfType<Options>();
			if (o != null) o.enabled = true;
			if (Object.FindObjectsOfType<TextMeshProUGUI>().Length == 0) return;
			foreach (TextMeshProUGUI t in Object.FindObjectsOfType<TextMeshProUGUI>())
			{
				if (t.text == "KARLSON")
				{
					t.text = "KARLSON\nTAS";
					break;
				}
			}
		}
		Time.timeScale = gameSpeed / 100f;
	}


	public override void OnUpdate() {
		base.OnUpdate();
		if (Input.GetKeyDown(savHotkey) & Time.timeScale != 0f) SetSaveState();
		else if (Input.GetKeyDown(loadHotkey) & Time.timeScale != 0f) MelonCoroutines.Start(GetSaveState());
		//InputPatch();
	}

	static void SetObjArray() {
		allMovables = new GameObject[Object.FindObjectsOfType<Rigidbody>().Length];
		int counter = 0;
		foreach (Rigidbody r in Object.FindObjectsOfType<Rigidbody>())
		{
			allMovables[counter] = r.gameObject;
			counter++;
		}
		allEnemies = Object.FindObjectsOfType<Enemy>();
	}

	private static Transform[] temp;
	public static void Reload() { // weirdass shenanigan that fixes the glitch where keys are not registered in scene transition for savestates
		temp = GameObject.FindObjectsOfType(typeof(Transform)) as Transform[];
		foreach (Transform t in temp)
		{
			if (t.gameObject.scene.buildIndex >= 0) GameObject.Destroy(t.gameObject);
		}
#pragma warning disable CS0618
		Application.LoadLevelAdditive(Application.loadedLevel);
	}

	IEnumerator InitDebug() {
		yield return null;
		Debug d = Object.FindObjectOfType<Debug>();
		typeof(Debug).GetField("fpsOn").SetValue(d, true);
		typeof(Debug).GetField("speedOn").SetValue(d, true);
		GameState.Instance.SetSlowmo(false);
	}

	static string VersionToString(float version) {
		string foo = (version % 10).ToString();
		string bar = (Mathf.Floor(version) / 10).ToString();
		return bar + "." + foo;
	}

	public static void SetSaveState() {
		IniFile ini = new IniFile(Directory.GetCurrentDirectory() + "\\savestates\\savestate.ini");
		PlayerMovement plr = Object.FindObjectOfType<PlayerMovement>();
		Vector3 position = plr.gameObject.transform.position;
		Vector3 vel = plr.gameObject.GetComponent<Rigidbody>().velocity;
		string section = "player";
		ini.SetFloat(section, "Version", version);
		ini.SetString(section, "Level", SceneManager.GetActiveScene().name);
		ini.SetFloat(section, "Time", Timer.Instance.GetTimer());
		ini.SetFloat(section, "PlayerX", position.x);
		ini.SetFloat(section, "PlayerY", position.y);
		ini.SetFloat(section, "PlayerZ", position.z);
		ini.SetFloat(section, "VelocityX", vel.x);
		ini.SetFloat(section, "VelocityY", vel.y);
		ini.SetFloat(section, "VelocityZ", vel.z);
		ini.SetFloat(section, "CameraX", plr.playerCam.rotation.eulerAngles.x);
		ini.SetFloat(section, "CameraY", plr.playerCam.rotation.eulerAngles.y);
		ini.SetFloat(section, "CameraZ", plr.playerCam.rotation.eulerAngles.z);
		// xrotation might have some slight difference from the camera x which i'm not fully aware of
		FieldInfo internalX = typeof(PlayerMovement).GetField("xRotation", BindingFlags.NonPublic | BindingFlags.Instance); 
		ini.SetFloat(section, "InternalCameraX", (float)internalX.GetValue(plr));
		FieldInfo gun = typeof(DetectWeapons).GetField("gun", BindingFlags.NonPublic | BindingFlags.Instance);
		int gunIndex = -1;
		for (int i = 0; i < allMovables.Length; i++)
		{
			if (allMovables[i] == plr.gameObject) continue;
			section = "obj_" + i.ToString();
			ini.SetBool(section, "isDestroyed", allMovables[i] == null);
			if (allMovables[i] == null) continue;
			ini.SetFloat(section, "PositionX", allMovables[i].transform.position.x);
			ini.SetFloat(section, "PositionY", allMovables[i].transform.position.y);
			ini.SetFloat(section, "PositionZ", allMovables[i].transform.position.z);
			ini.SetFloat(section, "RotationX", allMovables[i].transform.eulerAngles.x);
			ini.SetFloat(section, "RotationY", allMovables[i].transform.eulerAngles.y);
			ini.SetFloat(section, "RotationZ", allMovables[i].transform.eulerAngles.z);
			if (allMovables[i] == (GameObject)gun.GetValue(Object.FindObjectOfType<DetectWeapons>())) gunIndex = i;
			Rigidbody rb = allMovables[i].GetComponent<Rigidbody>();
			if (rb == null) continue; 
			ini.SetFloat(section, "VelocityX", rb.velocity.x);
			ini.SetFloat(section, "VelocityY", rb.velocity.y);
			ini.SetFloat(section, "VelocityZ", rb.velocity.z);
		}
		ini.SetInt("Player", "WeaponId", gunIndex);
		for (int i = 0; i < allEnemies.Length; i++) //TODO: complete enemy saving
		{
			if (allEnemies[i].gameObject == null) continue;
			section = "enemy_" + i.ToString();
			ini.SetFloat(section, "PositionX", allEnemies[i].gameObject.transform.position.x);
			ini.SetFloat(section, "PositionY", allEnemies[i].gameObject.transform.position.y);
			ini.SetFloat(section, "PositionZ", allEnemies[i].gameObject.transform.position.z);
			ini.SetFloat(section, "Rotation", allEnemies[i].gameObject.transform.eulerAngles.y);
		}
		if (SceneManager.GetActiveScene().buildIndex == 12)
		{
			section = "Sky2";
			Transform washingMachine = Object.FindObjectOfType<RotateObject>().gameObject.transform;
			ini.SetFloat(section, "RotationX", washingMachine.eulerAngles.x);
			ini.SetFloat(section, "RotationYZ", washingMachine.eulerAngles.y);
		}
		allBullets = Object.FindObjectsOfType<Bullet>();
		ini.SetInt("Player", "BulletCount", allBullets.Length);
		for (int i = 0; i < allBullets.Length; i++)
        {
			section = "bullet_" + i;
			ini.SetFloat(section, "PositionX", allBullets[i].gameObject.transform.position.x);
			ini.SetFloat(section, "PositionY", allBullets[i].gameObject.transform.position.y);
			ini.SetFloat(section, "PositionZ", allBullets[i].gameObject.transform.position.z);
			ini.SetFloat(section, "RotationX", allBullets[i].gameObject.transform.eulerAngles.x);
			ini.SetFloat(section, "RotationY", allBullets[i].gameObject.transform.eulerAngles.y);
			ini.SetFloat(section, "RotationZ", allBullets[i].gameObject.transform.eulerAngles.z);
			Rigidbody rb = allBullets[i].gameObject.GetComponentInChildren<Rigidbody>();
			ini.SetFloat(section, "VelocityX", rb.velocity.x);
			ini.SetFloat(section, "VelocityY", rb.velocity.y);
			ini.SetFloat(section, "VelocityZ", rb.velocity.z);
		}
	}

	public static IEnumerator GetSaveState(string name = "savestate") {
		//Time.timeScale = 0f;
		IniFile ini = new IniFile(Directory.GetCurrentDirectory() + "\\savestates\\" + name + ".ini");
		string section = "player";
		if (ini.GetString(section, "Level") != SceneManager.GetActiveScene().name)
		{
			MelonLogger.Warning("This savestate is not for this level!");
			yield break;
		}
		else if (ini.GetFloat(section, "Version") != version)
		{
			MelonLogger.Warning("This savestate is for another version of Karlson TAS!");
			yield break;
		}
		Reload();
		yield return null;
		SetObjArray();
		PlayerMovement plr = Object.FindObjectOfType<PlayerMovement>();
		Transform transform = plr.gameObject.transform;
		float X = ini.GetFloat(section, "PlayerX");
		float Y = ini.GetFloat(section, "PlayerY");
		float Z = ini.GetFloat(section, "PlayerZ");
		transform.position = new Vector3(X, Y, Z);
		X = ini.GetFloat(section, "CameraX");
		Y = ini.GetFloat(section, "CameraY");
		Z = ini.GetFloat(section, "CameraZ");
		plr.playerCam.eulerAngles = new Vector3(X, Y, Z);
		FieldInfo internalX = typeof(PlayerMovement).GetField("xRotation", BindingFlags.NonPublic | BindingFlags.Instance);
		internalX.SetValue(plr, ini.GetFloat(section, "InternalCameraX")); 
		plr.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(ini.GetFloat(section, "VelocityX"), ini.GetFloat(section, "VelocityY"), ini.GetFloat(section, "VelocityZ"));
		X = ini.GetInt(section, "WeaponId");
		if (X >= 0) Object.FindObjectOfType<DetectWeapons>().ForcePickup(allMovables[(int)X]);
		if (ini.GetString(section, "Level").Equals("10Sky2"))
		{
			Object.FindObjectOfType<RotateObject>().gameObject.transform.eulerAngles = new Vector3(
				ini.GetFloat("Sky2", "RotationX"),
				ini.GetFloat("Sky2", "RotationYZ"),
				ini.GetFloat("Sky2", "RotationYZ"));
		}
		for (int i = 0; i < allMovables.Length; i++) 
		{
			if (allMovables[i] == plr.gameObject) continue;
			section = "obj_" + i.ToString();
			if (ini.GetBool(section, "isDestroyed"))
			{
				Object.Destroy(allMovables[i]);
				continue;
			}
			X = ini.GetFloat(section, "PositionX");
			Y = ini.GetFloat(section, "PositionY");
			Z = ini.GetFloat(section, "PositionZ");
			allMovables[i].transform.position = new Vector3(X, Y, Z);
			X = ini.GetFloat(section, "RotationX");
			Y = ini.GetFloat(section, "RotationY");
			Z = ini.GetFloat(section, "RotationZ");
			allMovables[i].transform.eulerAngles = new Vector3(X, Y, Z);
			X = ini.GetFloat(section, "VelocityX");
			Y = ini.GetFloat(section, "VelocityY");
			Z = ini.GetFloat(section, "VelocityZ");
			allMovables[i].GetComponent<Rigidbody>().velocity = new Vector3(X, Y, Z);
		}
		FieldInfo time = typeof(Timer).GetField("timer", BindingFlags.NonPublic | BindingFlags.Instance);
		time.SetValue(Timer.Instance, ini.GetFloat("player", "Time"));
		for (int i = 0; i < allEnemies.Length; i++)
		{
			section = "enemy_" + i;
			X = ini.GetFloat(section, "PositionX");
			Y = ini.GetFloat(section, "PositionY");
			Z = ini.GetFloat(section, "PositionZ");
			allEnemies[i].transform.position = new Vector3(X, Y, Z);
			allEnemies[i].transform.eulerAngles = new Vector3(
				allEnemies[i].transform.eulerAngles.x, 
				ini.GetFloat(section, "Rotation"), 
				allEnemies[i].transform.eulerAngles.z);
		}
		for (int i = 0; i < ini.GetInt("Player", "BulletCount"); i++)
        {
			section = "bullet_" + i;
			X = ini.GetFloat(section, "PositionX");
			Y = ini.GetFloat(section, "PositionY");
			Z = ini.GetFloat(section, "PositionZ");
			float RX = ini.GetFloat(section, "RotationX");
			float RY = ini.GetFloat(section, "RotationY");
			float RZ = ini.GetFloat(section, "RotationZ");
			GameObject b = Object.Instantiate(bulletPrefab, new Vector3(X, Y, Z), Quaternion.identity);
			X = ini.GetFloat(section, "VelocityX");
			Y = ini.GetFloat(section, "VelocityY");
			Z = ini.GetFloat(section, "VelocityX");
			b.GetComponentInChildren<Rigidbody>().velocity = new Vector3(X, Y, Z);
		}
	}

}

