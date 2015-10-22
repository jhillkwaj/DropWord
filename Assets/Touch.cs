using UnityEngine;
using System.Collections;

public class Touch : MonoBehaviour {
	public bool mouseDown;
	public GameObject Tile = null;
	public Vector3 position;
	public float dropY;
	public float maxDrop;
	public float dropLeft;
	public float dropRight;
	public bool drag = false;

	public ArrayList letters = new ArrayList();

	public Vector3 startTilePos;

	public GameObject wordText;

	string[] specialWords = {"fox","call","wow","die","fuck","shit","kill","thanks","bacon","cat","dog","squirrel"};
	string[] specialAlts = {"ring-ding-ding","...me maybe?","so words, much fall","No need to get angry","No need to get angry","No need to get angry","that or be killed",
		"...for all the fish","mmm...bacon","human overlord","squirrel","furry rat"};


	public bool onlast = false;

	public void Start()
	{
		dropY += .38f;
		dropLeft -= .5f;
		dropRight -= .3f;
	}
	
	// Update is called once per frame
	void Update () {

				if (Time.timeScale > 0) {
						try {
								if ((Input.GetMouseButtonDown (0) || Input.GetTouch (0).phase == TouchPhase.Began)) {
										Vector3 clickedPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
										Collider2D[] arr = Physics2D.OverlapCircleAll (clickedPosition, .05f);
										foreach (Collider2D g in arr) {
												if (g.transform.gameObject.tag == "Tile"||g.transform.gameObject.tag == "Droppable") {
														Tile = g.gameObject;
														position = clickedPosition;
												}
										}
										if (Tile != null) {
												if (Tile.GetComponent<Drag> ().active == false) {
														Tile.GetComponent<Drag> ().selected = true;
														mouseDown = true;
														startTilePos = Tile.transform.position;
												} else if (Tile.transform.rigidbody2D.velocity.magnitude < .01f) {
														drag = true;
														letters.Add (Tile);
														Tile.GetComponent<SpriteRenderer> ().sprite = Tile.GetComponent<LetterInteract> ().selected;
														wordText.GetComponent<TextMesh> ().text = Tile.GetComponent<Drag> ().Letter.GetComponent<TextMesh> ().text;
												} else {
														Tile = null;
												}
										}
								}
						} catch (UnityException e) {
						}
			//move when dragged
						if (mouseDown&&!Tile.GetComponent<Drag>().active) {
								Vector3 clickedPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
								Vector3 movement = new Vector3 (clickedPosition.x - position.x, clickedPosition.y - position.y, clickedPosition.z - position.z);
								Vector3 tilePostion = Tile.transform.position;
								Tile.transform.position = new Vector3 (tilePostion.x + movement.x, tilePostion.y + movement.y, tilePostion.z + movement.z);
								position = clickedPosition;
				//out of drag area
								if (Tile.transform.position.y < dropY + (Tile.collider2D.renderer.bounds.size.y/2)) {
										Tile.transform.position = new Vector3 (tilePostion.x + movement.x, dropY + (Tile.collider2D.renderer.bounds.size.y/2), tilePostion.z + movement.z);
								}
				if(Tile.transform.position.x-(Tile.collider2D.renderer.bounds.size.x/2)<dropLeft)
								{
					Tile.transform.position = new Vector3 (dropLeft+(Tile.collider2D.renderer.bounds.size.x/2), tilePostion.y + movement.y, tilePostion.z + movement.z);
								}
								if(Tile.transform.position.x+(Tile.collider2D.renderer.bounds.size.x/2)>dropRight)
								{
					                    Tile.transform.position = new Vector3 (dropRight-(Tile.collider2D.renderer.bounds.size.x/2), tilePostion.y + movement.y, tilePostion.z + movement.z);
								}
						}
			//if it droped
			else if(mouseDown&&Tile.GetComponent<Drag>().dropOdds>1 && Tile.GetComponent<Drag>().active)
			{
				Tile = null;
				mouseDown=false;
			}


			//connect letters
						if (drag) {
								Tile = null;
								onlast = false;
								Vector3 clickedPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
								Collider2D[] arr = Physics2D.OverlapCircleAll (clickedPosition, .05f);
								foreach (Collider2D g in arr) {
										if (g.transform.gameObject.tag == "Tile") {
												Tile = g.gameObject;

												string word = "";
												foreach (GameObject f in letters) {
														word += f.GetComponent<Drag> ().Letter.GetComponent<TextMesh> ().text;
												}
												wordText.GetComponent<TextMesh> ().text = word;
										}
								}
								if (Tile != null) { //the finger is on a tile
										if (Tile.transform.rigidbody2D.velocity.magnitude < .01f) { //its not moveing
												if (((GameObject)letters [letters.Count - 1]).GetComponent<LetterInteract> ().contains (Tile)) {
														if (!letters.Contains (Tile)) {
																letters.Add (Tile);
																Tile.GetComponent<SpriteRenderer> ().sprite = Tile.GetComponent<LetterInteract> ().selected;
														}
														
												}
												else if(letters[letters.Count-1]==Tile) //see if the finger is on the last tile
												{
													onlast = true;
												}
										}
								}
						}

						try {
								if ((Input.GetMouseButtonUp (0) || Input.GetTouch (0).phase == TouchPhase.Ended)) {
										if (mouseDown) {
						//drop
												if (Tile.transform.position.y < maxDrop) {
													this.GetComponent<Spawn> ().newBlock (startTilePos);
												}
												else{ 
							//to high to drop
													Tile.transform.position = startTilePos;
													Tile.GetComponent<Drag>().active = false;
													Tile.GetComponent<Drag>().selected = false;
													Tile.GetComponent<Rigidbody2D>().isKinematic=true;
												}
												
										}
										Tile = null;
										mouseDown = false;
										drag = false;
										
					//check for a word
											if (letters.Count > 2&&onlast) { //if greater then two letters in length and on the last tile
												string word = "";
												foreach (GameObject g in letters) {
														word += g.GetComponent<Drag> ().Letter.GetComponent<TextMesh> ().text;
														g.GetComponent<SpriteRenderer> ().sprite = g.GetComponent<LetterInteract> ().defalt;
												}
												if (this.GetComponent<Dictionary> ().contains (word.ToLower ())) { //if its a word
							if(word.Contains("*"))
							{
								wordText.GetComponent<TextMesh> ().text = this.GetComponent<Dictionary> ().lastWord.ToUpper();
							}
														AudioSource.PlayClipAtPoint((AudioClip)Resources.Load ("ping"), GameObject.FindGameObjectWithTag("MainCamera").transform.position);
							if(!word.Contains("*"))
							{
														this.GetComponent<SetScore> ().score (word);
							}
														spawnCoins();
														foreach (GameObject g in letters) {
																Destroy (g.gameObject);
														}


														for (int i = 0; i < specialWords.Length; i++) {
															if(specialWords[i]==word.ToLower()&&Random.value<.2f)
															{
																wordText.GetComponent<TextMesh> ().text = specialAlts[i].ToUpper();
															}
														}
												} else { //if its not a word
														//Debug.Log ("Not a Word:" + word);
														wordText.GetComponent<TextMesh> ().text = "";
												}
										} else { //less then two letter in lenghth or not on the last tile
												foreach(GameObject g in letters)
												{
												g.GetComponent<SpriteRenderer> ().sprite = g.GetComponent<LetterInteract> ().defalt;
												}
												wordText.GetComponent<TextMesh> ().text = "";
												
										}
										letters.Clear ();
								}
						} catch (UnityException e) {

						}


				}
		}


	public void spawnCoins()
	{
		for (int i = 0; i < Mathf.Floor(Mathf.Pow(letters.Count-1,2)); i++) {
			GameObject letter = (GameObject)letters[Random.Range(0,letters.Count)];
			Vector2 randomPos = Random.insideUnitCircle;

			GameObject coin = (GameObject)Instantiate (Resources.Load ("Coin"));
			coin.transform.position = new Vector3(letter.transform.position.x+randomPos.x,letter.transform.position.y+randomPos.y,Random.Range(-.5f,-6f));
				}
		GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<FailRefrence> ().addCoins ((int)Mathf.Floor(Mathf.Pow(letters.Count-1,2)));
	}
}
