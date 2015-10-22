using UnityEngine;
using System.Collections;

public class PauseMenuClick : MonoBehaviour {

	public bool menu;
	public int menuLevel;

	public bool resume;
	public GameObject hideOrShow = null;

	public bool restart;

	public bool showMenu;

	public bool nextLevel;

	public string key;



	public string loadScene = "";

	void OnMouseDown() {
		if (resume) {
			hideOrShow.gameObject.SetActive(false);
			Time.timeScale = 1;
				}

		if (restart) {
			Application.LoadLevel(Application.loadedLevel);
				}

		if (menu) {
			Application.LoadLevel(menuLevel);
				}

		if (showMenu && Time.timeScale>0) {
			hideOrShow.gameObject.SetActive(true);
			Time.timeScale = 0;
				}

		if (nextLevel) {
			Application.LoadLevel(Application.loadedLevel+1);
				}

		if (loadScene.Length > 1) {
			Application.LoadLevel(loadScene);
				}


	}


	void Start () {
		if(key!="" && PlayerPrefs.HasKey("savedGame")){
			string level = PlayerPrefs.GetString("savedGame");
			
			if(level.Contains(" "+key+" "))
			{
				this.GetComponent<TextMesh>().color = new Color(0,230,0);
				return;
			}
		}
		
	}
}
