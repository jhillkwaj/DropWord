using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class Destroy2DNode : ScriptableObject{
	
	public int type = 0;
	public bool isGeometry = false;
	public bool isFull = false;
	public float size = 0;
	public float terrainSize = 0;
	
	private bool nodeEnabled = true;
	public float tl;
	public float tr;
	public float bl;
	public float br;
	
	public Vector3 nodePosition;
	
	public Vector3[] vertices = null;
	public Vector2[] uv = null;
	public Vector2[] uv1 = null;
	public int[] indexes = null;
	
	private Vector3 planeCenter = Vector3.zero;
	private Vector3 planeNormal = Vector3.zero;
	private float planeDistance = 0;
	
	private Vector3 planeCenter2 = Vector3.zero;
	private Vector3 planeNormal2 = Vector3.zero;
	private float planeDistance2 = 0;
	public bool hasSecondNormal = false;
	
	public bool initialized = false;
	private float _u;
	private float _v;
	private float _sizeUV;
	
	public bool tl_on;
	public bool tr_on;
	public bool bl_on;
	public bool br_on;
	
	public void OnEnable() {
		hideFlags = HideFlags.HideInHierarchy;
	}
	
	public void drawDebug() {
		
		if (nodeEnabled) {
			
			Debug.DrawLine(nodePosition						 , nodePosition + Vector3.right * size);
			Debug.DrawLine(nodePosition						 , nodePosition + Vector3.down * size);
			Debug.DrawLine(nodePosition + Vector3.right * size , nodePosition + Vector3.right * size + Vector3.down * size);
			Debug.DrawLine(nodePosition + Vector3.down * size  , nodePosition + Vector3.right * size + Vector3.down * size);
			Debug.DrawLine(nodePosition 						 , nodePosition + Vector3.right * size + Vector3.down * size);
			Debug.DrawLine(nodePosition + Vector3.down * size  , nodePosition + Vector3.right * size);
			
		}
		
	}
	
	public void drawDebugNormal() {
		
		if (nodeEnabled) {
			Debug.DrawLine(nodePosition + planeCenter			 , nodePosition + planeCenter + planeNormal * size*0.25f);
		}
		
	}
	
	public void setValues(float u, float v, float sizeUV, float nodeSize) {
		size = nodeSize;
		_u = u;
		_v = v;
		_sizeUV = sizeUV;
		initialized = false;
	}
	
	public void initDelayed(float colorValue, Material mat) {
		if (initialized) return;
		init (null, _u, _v, _sizeUV, size, colorValue, mat);
	}
	
	public bool init(Texture2D mapTexture, float u, float v, float sizeUV, float nodeSize, float colorValue, Material mat) {
		
		size = nodeSize;
		
		//nodeEnabled = !checkAllZero(mapTexture, u, v, sizeUV);
		
		if (mapTexture != null) {
			Color tlc = mapTexture.GetPixelBilinear(u,v);
			Color trc = mapTexture.GetPixelBilinear(u+sizeUV,v);
			Color blc = mapTexture.GetPixelBilinear(u,v-sizeUV);
			Color brc = mapTexture.GetPixelBilinear(u+sizeUV,v-sizeUV);
			
			tl = Mathf.Clamp01(tlc.a);
			tr = Mathf.Clamp01(trc.a);
			bl = Mathf.Clamp01(blc.a);
			br = Mathf.Clamp01(brc.a); 
		} else {
			
			tl = 0;
			tr = 0;
			bl = 0;
			br = 0;
		}
		
		//bool allOver  = (tl >= colorValue) && (tr >= colorValue) && (bl >= colorValue) && (br >= colorValue);
		bool allUnder = (tl < colorValue) && (tr < colorValue) && (bl < colorValue) && (br < colorValue);
		
		nodeEnabled = !allUnder;
		
		isGeometry = false;
		isFull = false;
		
		if (nodeEnabled) createGeometry(colorValue);
		
		initialized = true;
		
		return isGeometry || isFull;
		
	}
	
	public void reInit(float colorValue) {
		bool allUnder = (tl < colorValue) && (tr < colorValue) && (bl < colorValue) && (br < colorValue);
		
		nodeEnabled = !allUnder;
		
		isGeometry = false;
		isFull = false;
		
		if (nodeEnabled) createGeometry(colorValue);
		
		initialized = true;
	}
	
	public static Destroy2DNode createNewNode() {
		/*GameObject newObject = new GameObject();
		return newObject.AddComponent<Destroy2DNode>();*/
		//return new Destroy2DNode();
		return ScriptableObject.CreateInstance<Destroy2DNode>();
	}
	
	private bool checkAllZero(Texture2D texture, float u, float v, float uvSize) {
		
		float delta = 1.0f / texture.width;
		
		for (float x = 0; x<uvSize; x+=delta) {
			if (texture.GetPixelBilinear(u+x, v).r != 0) return false;
			if (texture.GetPixelBilinear(u+x, v+uvSize).r != 0) return false;
			if (texture.GetPixelBilinear(u, v+x).r != 0) return false;
			if (texture.GetPixelBilinear(u+uvSize, v+x).r != 0) return false;
		}
		
		return true;
		
	}
	
	public bool checkCollision(Vector3 pos) {
		return (Vector3.Dot(planeNormal, pos) < planeDistance);
		
	}
	
	public void createGeometry(float colorValue) {
		
		/*MeshFilter mf = GetComponent<MeshFilter>();
		Mesh m;
		if (mf.sharedMesh != null) {
			m = mf.sharedMesh;
			m.Clear();
		} else {
			m = new Mesh();
			mf.mesh = m;
		}*/
		
		hasSecondNormal = false;
		
		vertices = null;
		uv = null;
		uv1 = null;
		indexes = null;
		
		tl_on = tl >= colorValue;
		tr_on = tr >= colorValue;
		bl_on = bl >= colorValue;
		br_on = br >= colorValue;
		
		float top = Mathf.Abs (tl - colorValue) / (Mathf.Abs(tl - tr));
		float down = Mathf.Abs (bl - colorValue) / (Mathf.Abs(bl - br));
		float left = Mathf.Abs (tl - colorValue) / (Mathf.Abs(tl - bl));
		float right = Mathf.Abs (tr - colorValue) / (Mathf.Abs(tr - br));
		
		float tluv = 1 - Mathf.Abs (tl - colorValue) / colorValue;
		float truv = 1 - Mathf.Abs (tr - colorValue) / colorValue;
		float bluv = 1 - Mathf.Abs (bl - colorValue) / colorValue;
		float bruv = 1 - Mathf.Abs (br - colorValue) / colorValue;
		
		
		//UM ////////////////////////////////////////////////////////////////////////////
		if (tl_on && !tr_on && !bl_on && !br_on) {
			
			vertices = new Vector3[3];
			uv = new Vector2[3];
			uv1 = new Vector2[3];
			indexes = new int[3];
			
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * top * size;
			vertices[2] = Vector3.down * left * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			/*uv[0] = Vector2.zero;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right;*/
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[2] - vertices[1];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[1]);
			planeCenter = vertices[1] + planeCenter / 2;
			
			type = 1;
			
		}
		
		if (!tl_on && tr_on && !bl_on && !br_on) {
			
			vertices = new Vector3[3];
			uv = new Vector2[3];
			uv1 = new Vector2[3];
			indexes = new int[3];
			
			vertices[0] = Vector3.right * top * size;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.down * right * size + Vector3.right * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[0] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[0]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			type = 2;
		}
		
		if (!tl_on && !tr_on && bl_on && !br_on) {
			
			vertices = new Vector3[3];
			uv = new Vector2[3];
			uv1 = new Vector2[3];
			indexes = new int[3];
			
			vertices[0] = Vector3.down * (left) * size;
			vertices[1] = Vector3.right * down * size + Vector3.down * size;
			vertices[2] = Vector3.down * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right * bluv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[0] - vertices[1];
			planeNormal = (new Vector3(planeCenter.y, -planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[0]);
			planeCenter = vertices[1] + planeCenter / 2;
			
			type = 3;
		}
		
		if (!tl_on && !tr_on && !bl_on && br_on) {
			
			vertices = new Vector3[3];
			uv = new Vector2[3];
			uv1 = new Vector2[3];
			indexes = new int[3];
			
			vertices[0] = Vector3.right * size + Vector3.down * right * size;
			vertices[1] = Vector3.right * size + Vector3.down * size;
			vertices[2] = Vector3.down * size + Vector3.right * size * down;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right * bruv;
			uv[2] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[0] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[0]);
			planeCenter = vertices[2] + planeCenter / 2;
			type = 4;
			
		}
		
		//DOIS //////////////////////////////////////////////////////////////////////////		
		if (tl_on && tr_on && !bl_on && !br_on) {
			
			vertices = new Vector3[4];
			uv = new Vector2[4];
			uv1 = new Vector2[4];
			indexes = new int[6];
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.down * right * size + Vector3.right * size;
			vertices[3] = Vector3.down * left * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 3;
			indexes[3] = 1;
			indexes[4] = 2;
			indexes[5] = 3;
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			
			planeCenter = vertices[2] - vertices[3];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[3]);
			planeCenter = vertices[3] + planeCenter / 2;
			
			type = 5;
			
		}
		
		if (!tl_on && !tr_on && bl_on && br_on) {
			
			vertices = new Vector3[4];
			uv = new Vector2[4];
			uv1 = new Vector2[4];
			indexes = new int[6];
			
			vertices[0] = Vector3.down * left * size;
			vertices[1] = Vector3.right * size + Vector3.down * right * size;
			vertices[2] = Vector3.down * size + Vector3.right * size;
			vertices[3] = Vector3.down * size;
			
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 3;
			indexes[3] = 2;
			indexes[4] = 3;
			indexes[5] = 1;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right * bruv;
			uv[3] = Vector2.right * bluv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			
			planeCenter = vertices[1] - vertices[0];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[1]);
			planeCenter = vertices[0] + planeCenter / 2;
			
			type = 6;
			
		}
		
		if (tl_on && !tr_on && bl_on && !br_on) {
			
			vertices = new Vector3[4];
			uv = new Vector2[4];
			uv1 = new Vector2[4];
			indexes = new int[6];
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * top * size;
			vertices[2] = Vector3.down * size + Vector3.right * down * size;
			vertices[3] = Vector3.down * size;
			
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 3;
			indexes[3] = 2;
			indexes[4] = 3;
			indexes[5] = 1;
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right * bluv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			
			planeCenter = vertices[3] - vertices[1];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[1]);
			planeCenter = vertices[1] + planeCenter / 2;
			
			type = 7;
		}
		
		if (!tl_on && tr_on && !bl_on && br_on) {
			
			vertices = new Vector3[4];
			uv = new Vector2[4];
			uv1 = new Vector2[4];
			indexes = new int[6];
			
			vertices[0] = Vector3.right * top * size;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.down * size + Vector3.right * size;
			vertices[3] = Vector3.down * size + Vector3.right * down * size;
			
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 3;
			indexes[3] = 2;
			indexes[4] = 3;
			indexes[5] = 1;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right * bruv;
			uv[3] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			
			planeCenter = vertices[0] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[2]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			type = 8;
		}
		
		//TRES //////////////////////////////////////////////////////////////////////////
		if (tl_on && tr_on && bl_on && !br_on) {
			
			vertices = new Vector3[5];
			uv = new Vector2[5];
			uv1 = new Vector2[5];
			indexes = new int[9];
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.right * size + Vector3.down * right * size;
			vertices[3] = Vector3.down * size + Vector3.right * down * size;
			vertices[4] = Vector3.down * size;
			
			planeCenter = vertices[3] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[2]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 0;
			indexes[4] = 2;
			indexes[5] = 3;
			indexes[6] = 0;
			indexes[7] = 3;
			indexes[8] = 4;
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right;
			uv[4] = Vector2.right * bluv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			
			type = 9;
			
		}
		
		if (tl_on && tr_on && !bl_on && br_on) {
			
			vertices = new Vector3[5];
			uv = new Vector2[5];
			uv1 = new Vector2[5];
			indexes = new int[9];
			
			vertices[0] = Vector3.right * size;
			vertices[1] = Vector3.right * size + Vector3.down * size;
			vertices[2] = Vector3.down * size + Vector3.right * down * size;
			vertices[3] = Vector3.down * left * size;
			vertices[4] = Vector3.zero;
			
			planeCenter = vertices[3] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[2]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 0;
			indexes[4] = 2;
			indexes[5] = 3;
			indexes[6] = 0;
			indexes[7] = 3;
			indexes[8] = 4;
			
			uv[0] = Vector2.right * truv;
			uv[1] = Vector2.right * bruv;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right;
			uv[4] = Vector2.right * tluv;		
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			
			type = 10;
			
		}
		
		if (tl_on && !tr_on && bl_on && br_on) {
			
			vertices = new Vector3[5];
			uv = new Vector2[5];
			uv1 = new Vector2[5];
			indexes = new int[9];
			
			vertices[0] = Vector3.down * size;
			vertices[1] = Vector3.zero;
			vertices[2] = Vector3.right * top * size;
			vertices[3] = Vector3.right * size + Vector3.down * right * size;
			vertices[4] = Vector3.right * size + Vector3.down * size;
			
			planeCenter = vertices[3] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[2]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 0;
			indexes[4] = 2;
			indexes[5] = 3;
			indexes[6] = 0;
			indexes[7] = 3;
			indexes[8] = 4;
			
			uv[0] = Vector2.right * bluv;
			uv[1] = Vector2.right * tluv;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right;
			uv[4] = Vector2.right * bruv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			
			type = 11;
			
		}
		
		if (!tl_on && tr_on && bl_on && br_on) {
			
			vertices = new Vector3[5];
			uv = new Vector2[5];
			uv1 = new Vector2[5];
			indexes = new int[9];
			
			vertices[0] = Vector3.right * size + Vector3.down * size;
			vertices[1] = Vector3.down * size;
			vertices[2] = Vector3.down * left * size;
			vertices[3] = Vector3.right * top *  size;
			vertices[4] = Vector3.right * size;
			
			planeCenter = vertices[3] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[2]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 0;
			indexes[4] = 2;
			indexes[5] = 3;
			indexes[6] = 0;
			indexes[7] = 3;
			indexes[8] = 4;
			
			uv[0] = Vector2.right * bruv;
			uv[1] = Vector2.right * bluv;
			uv[2] = Vector2.right;
			uv[3] = Vector2.right;
			uv[4] = Vector2.right * truv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			
			type = 12;
			
		}
		
		//DIAGONAIS
		if (tl_on && !tr_on && !bl_on && br_on) {
			vertices = new Vector3[6];
			uv = new Vector2[6];
			uv1 = new Vector2[6];
			indexes = new int[6];
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * top * size;
			vertices[2] = Vector3.down * left * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			/*uv[0] = Vector2.zero;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right;*/
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right;
			uv[2] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[2] - vertices[1];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[1]);
			planeCenter = vertices[1] + planeCenter / 2;
			
			///////
			
			vertices[3] = Vector3.right * size + Vector3.down * right * size;
			vertices[4] = Vector3.right * size + Vector3.down * size;
			vertices[5] = Vector3.down * size + Vector3.right * size * down;
			
			indexes[3] = 3;
			indexes[4] = 4;
			indexes[5] = 5;
			
			uv[3] = Vector2.right;
			uv[4] = Vector2.right * bruv;
			uv[5] = Vector2.right;
			
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			uv1[5] = nodePosition + vertices[5];
			
			planeCenter2 = vertices[3] - vertices[5];
			planeNormal2 = (new Vector3(-planeCenter2.y, planeCenter2.x, 0)).normalized;
			planeDistance2 = Vector3.Dot(planeNormal2, nodePosition + vertices[3]);
			planeCenter2 = vertices[5] + planeCenter2 / 2;
			
			hasSecondNormal = true;
			
			type = 13;
		}
		
		if (!tl_on && tr_on && bl_on && !br_on) {
			vertices = new Vector3[6];
			uv = new Vector2[6];
			uv1 = new Vector2[6];
			indexes = new int[6];
			
			vertices[0] = Vector3.right * top * size;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.down * right * size + Vector3.right * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			
			uv[0] = Vector2.right;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			
			planeCenter = vertices[0] - vertices[2];
			planeNormal = (new Vector3(-planeCenter.y, planeCenter.x, 0)).normalized;
			planeDistance = Vector3.Dot(planeNormal, nodePosition + vertices[0]);
			planeCenter = vertices[2] + planeCenter / 2;
			
			///////
			
			vertices[3] = Vector3.down * (left) * size;
			vertices[4] = Vector3.right * down * size + Vector3.down * size;
			vertices[5] = Vector3.down * size;
			
			indexes[3] = 3;
			indexes[4] = 4;
			indexes[5] = 5;
			
			uv[3] = Vector2.right;
			uv[4] = Vector2.right;
			uv[5] = Vector2.right * bluv;
			
			uv1[3] = nodePosition + vertices[3];
			uv1[4] = nodePosition + vertices[4];
			uv1[5] = nodePosition + vertices[5];
			
			planeCenter2 = vertices[3] - vertices[4];
			planeNormal2 = (new Vector3(planeCenter2.y, -planeCenter2.x, 0)).normalized;
			planeDistance2 = Vector3.Dot(planeNormal2, nodePosition + vertices[3]);
			planeCenter2 = vertices[4] + planeCenter2 / 2;
			
			hasSecondNormal = true;
			
			type = 14;
		}
		
		// TODOS
		if (tl_on && tr_on && bl_on && br_on) {
			
			vertices = new Vector3[4];
			uv = new Vector2[4];
			uv1 = new Vector2[4];
			indexes = new int[6];
			
			vertices[0] = Vector3.zero;
			vertices[1] = Vector3.right * size;
			vertices[2] = Vector3.down * size;
			vertices[3] = Vector3.down * size + Vector3.right * size;
			
			indexes[0] = 0;
			indexes[1] = 1;
			indexes[2] = 2;
			indexes[3] = 3;
			indexes[4] = 2;
			indexes[5] = 1;
			
			uv[0] = Vector2.right * tluv;
			uv[1] = Vector2.right * truv;
			uv[2] = Vector2.right * bluv;
			uv[3] = Vector2.right * bruv;
			
			uv1[0] = nodePosition + vertices[0];
			uv1[1] = nodePosition + vertices[1];
			uv1[2] = nodePosition + vertices[2];
			uv1[3] = nodePosition + vertices[3];
			
			isFull = true;
			
			type = 15;
			
		}	
		
		if (uv1 != null) {
			for (int i=0; i<uv1.Length; i++) {
				uv1[i].x /= terrainSize;
				uv1[i].y = (uv1[i].y/terrainSize) - 1;
			}
		}
		
		isGeometry = ((type > 0) && (type < 15));
		
		/*m.vertices = vertices;
		m.triangles = indexes;
		m.uv = uv;
		m.uv1 = uv1;*/
		
	}
	
	public bool isEnabled() {
		return nodeEnabled;
	}
	
	public Vector3 getPlaneNormal() {
		return planeNormal;
	}
	
	public float getPlaneDistance() {
		return planeDistance;
	}
	
	public Vector3 getPlaneNormal2() {
		return planeNormal2;
	}
	
	public float getPlaneDistance2() {
		return planeDistance2;
	}
	
	public void addPolygonCollider(PolygonCollider2D polyCollider) { 
		
		if (type < 1) return;
		if (type > 14) return; 
		
		Vector2[] points = null;
		Vector2 nodeOffset = new Vector2(nodePosition.x - polyCollider.transform.localPosition.x, nodePosition.y - polyCollider.transform.localPosition.y);
		
		if ((type >=1) && (type <=12)) {
			
			points = new Vector2[vertices.Length];
			
			for (int i=0; i<vertices.Length; i++) {
				points[i] = new Vector2(vertices[i].x, vertices[i].y) + nodeOffset;
			}
			
			polyCollider.SetPath(polyCollider.pathCount-1, points);
			polyCollider.pathCount = polyCollider.pathCount+1;
			
		}
		
		if ((type == 13) || (type == 14)) {
			points = new Vector2[3];
			
			for (int i=0; i<3; i++) {
				points[i] = new Vector2(vertices[i].x, vertices[i].y) + nodeOffset;
			}
			
			polyCollider.SetPath(polyCollider.pathCount-1, points);
			polyCollider.pathCount = polyCollider.pathCount+1;
			
			points = new Vector2[3];
			
			for (int i=3; i<6; i++) {
				points[i-3] = new Vector2(vertices[i].x, vertices[i].y) + nodeOffset;
			}
			
			polyCollider.SetPath(polyCollider.pathCount-1, points);
			polyCollider.pathCount = polyCollider.pathCount+1;
		}
		
	}
	
}
