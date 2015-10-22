using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Destroy2D : MonoBehaviour {
	
	public float terrainSize = 50;
	public int resolution = 10;
	private float colorValue = 0.5f;
	public Shader shader;
	public Vector3 cameraPosition;
	public float renderDistanceFromCamera = 2;
	public int nodesPerMesh = 20;
	public float colliderDepth = 1;
	public bool debug = false;
	public bool debugNormal = false;
	public float brushSize = 2;
	public float brushIntensity = 0.75f;
	public int sortingLayer;
	public int sortingOrder;

	
	[SerializeField]
	public Destroy2DNode[] nodes;
	[SerializeField]
	public Destroy2DMesh[] nodeMeshes;
	[SerializeField]
	private Material m_material;
	[SerializeField]
	private Texture2D colorMasks;
	
	public Material material {
		get { 
			return m_material;
		}
		set { 
				m_material = value;
				updateMaterial();
			}
	}
	
	public void updateMaterial() {
		
		if (m_material == null) return;
		
		m_material.SetTexture("_ColorMask", colorMasks);
		
		foreach (Destroy2DMesh mesh in GetComponentsInChildren<Destroy2DMesh>()) {
			if (mesh.renderer != null) {
				mesh.renderer.sharedMaterial = m_material;
			}
		}

	}
	
	// Update is called once per frame
	void Update () {
		
		updateMaterial();
		
	}
	
	void drawDebug() {
		
		Debug.DrawLine(transform.position, transform.position + Vector3.right * terrainSize);
		Debug.DrawLine(transform.position, transform.position + Vector3.down * terrainSize);
		Debug.DrawLine(transform.position + Vector3.right * terrainSize , transform.position + Vector3.right * terrainSize + Vector3.down * terrainSize);
		Debug.DrawLine(transform.position + Vector3.down * terrainSize  , transform.position + Vector3.right * terrainSize + Vector3.down * terrainSize);
		
	}
	
	void OnDrawGizmos() {
		drawDebug();
	}
	
	void createTerrain(Texture2D mapTexture) {
		clean();
		
		createColorMask();
		
		nodes = new Destroy2DNode[resolution * resolution];
		
		int meshCount = Mathf.FloorToInt(resolution / nodesPerMesh);
		if ((resolution % nodesPerMesh) > 0) meshCount++;
		nodeMeshes = new Destroy2DMesh[meshCount * meshCount];
		Destroy2DMesh currentMesh = null;
		
		//nodeValuesTexture = new Texture2D(resolution, resolution);
		
		float nodeSize = terrainSize / resolution;
		float uvDelta = 1.0f / resolution;
		
		for (int x = 0; x < resolution; x++) {
			for (int y = 0; y < resolution; y++) {
				Destroy2DNode node = Destroy2DNode.createNewNode();
				node.terrainSize = terrainSize;
				
				node.nodePosition = new Vector3(x*nodeSize, y*-nodeSize, 0);
				
				nodes[x + y*resolution] = node;
				
				currentMesh = nodeMeshes[Mathf.FloorToInt(x/nodesPerMesh) + Mathf.FloorToInt(y/nodesPerMesh) * meshCount];
				
				if (currentMesh == null) {
					currentMesh = Destroy2DMesh.instantiate();
					currentMesh.gameObject.transform.parent = gameObject.transform;
					currentMesh.gameObject.transform.localPosition = node.nodePosition;
					
					nodeMeshes[Mathf.FloorToInt(x/nodesPerMesh) + Mathf.FloorToInt(y/nodesPerMesh) * meshCount] = currentMesh;
					
					currentMesh.meshX = x;
					currentMesh.meshY = y;
					currentMesh.meshNodeCount = nodesPerMesh;
				}
				
				if (node.init(mapTexture, x*uvDelta, 1 - y*uvDelta, uvDelta, nodeSize, colorValue, material)) {
					
				}
				
				//nodeValuesTexture.SetPixel(x,-y, Color.white * (colorValue + (node.tl + node.tr + node.bl + node.br) * 0.25f)); 
				
			}
		}
		
		//nodeValuesTexture.Apply();
		
		foreach (Destroy2DMesh mesh in GetComponentsInChildren<Destroy2DMesh>()) {
			mesh.commit(material, colliderDepth, sortingLayer, sortingOrder, this);
		}
		
	}
	
	void clean() {
		for (int i = transform.childCount-1; i>=0; i--) {
			GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
		}
		
		if (colorMasks != null) {
			Texture2D.DestroyImmediate(colorMasks);
			colorMasks = null;
		}
		
	}
	
	public void destroyAt(Vector3 pos, float radius, float intensity) {
		changeTerrain(pos, radius, intensity);
		commit ();
	}
	
	public void destroyAtEditor(Vector3 pos, float radius, float intensity) {
		changeTerrain(pos, radius, intensity);
		commitEditor();
	}
	
	private void commit() {
		foreach (Destroy2DMesh mesh in GetComponentsInChildren<Destroy2DMesh>()) {
			if (mesh.isDirty) {
							
				mesh.commit(material, colliderDepth, sortingLayer, sortingOrder, this);
			}
		}
	}
	
	private void commitEditor() {
		foreach (Destroy2DMesh mesh in GetComponentsInChildren<Destroy2DMesh>()) {
			if (mesh.isDirty) {
							
				mesh.commitEditor(material, colliderDepth, sortingLayer, sortingOrder, this);
			}
		}
	}
	
	private void changeTerrain(Vector3 pos, float radius, float intensity) {
		
		int meshCount = Mathf.FloorToInt(resolution / nodesPerMesh);
		Destroy2DMesh currentMesh = null;

		float nodeSize = terrainSize / resolution;
		int radiusSize = Mathf.FloorToInt(radius / nodeSize) + 1;
		int posX = Mathf.FloorToInt((pos.x - transform.position.x) / nodeSize);
		int posY = Mathf.FloorToInt(-(pos.y - transform.position.y) / nodeSize);
		float uvDelta = 1.0f / resolution;
		
		for (int x = posX - radiusSize; x < posX + radiusSize; x++) {
			for (int y = posY - radiusSize; y < posY + radiusSize; y++) {
				if ((x>=0) && (x<resolution) && (y>=0) && (y<resolution)) {
					Destroy2DNode node = nodes[x + y*resolution];
					
					if (node == null) {
						node = nodes[x + y*resolution] = Destroy2DNode.createNewNode();
						node.terrainSize = terrainSize;
						node.setValues(x*uvDelta, 1 - y*uvDelta, uvDelta, nodeSize);
						node.nodePosition = new Vector3(x*nodeSize, y*-nodeSize, 0);
					}
					
					if (node != null) {
							
						bool shouldUpdate = false;
						
						float rad_tl = ((node.nodePosition + transform.position ) - pos).magnitude;
						float rad_tr = ((node.nodePosition + transform.position + Vector3.right * nodeSize) - pos).magnitude;
						float rad_bl = ((node.nodePosition + transform.position + Vector3.down * nodeSize) - pos).magnitude;
						float rad_br = ((node.nodePosition + transform.position + Vector3.right * nodeSize + Vector3.down * node.size) - pos).magnitude;
						
						if (rad_tl < radius) {							
							node.tl -= intensity * (1 - (rad_tl / radius));
							shouldUpdate = true;
						}
						
						if (rad_tr < radius) {
							node.tr -= intensity * (1 - (rad_tr / radius));
							shouldUpdate = true;
						}
						
						if (rad_bl < radius) {
							node.bl -= intensity * (1 - (rad_bl / radius));
							shouldUpdate = true;
						}
						
						if (rad_br < radius) {
							node.br -= intensity * (1 - (rad_br / radius));
							shouldUpdate = true;
						}
						
						if (shouldUpdate) {
							
							node.tl = Mathf.Clamp01(node.tl);
							node.tr = Mathf.Clamp01(node.tr);
							node.bl = Mathf.Clamp01(node.bl);
							node.br = Mathf.Clamp01(node.br);
							
							currentMesh = nodeMeshes[Mathf.FloorToInt(x/nodesPerMesh) + Mathf.FloorToInt(y/nodesPerMesh) * meshCount];
							
							currentMesh.isDirty = true;
							
							Destroy2DNode otherNode = null;
							Destroy2DMesh otherMesh = null;
							
							//top left
							if ((x>1) && (y>1)) {
								otherNode = nodes[(x-1) + (y-1)*resolution];
								otherNode.br = node.tl;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x-1)/nodesPerMesh) + Mathf.FloorToInt((y-1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//top
							if (y>1) {
								otherNode = nodes[(x) + (y-1)*resolution];
								otherNode.bl = node.tl;
								otherNode.br = node.tr;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x)/nodesPerMesh) + Mathf.FloorToInt((y-1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//top right
							if ((x < (resolution-2)) && (y>1)) {
								otherNode = nodes[(x+1) + (y-1)*resolution];
								otherNode.bl = node.tr;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x+1)/nodesPerMesh) + Mathf.FloorToInt((y-1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//right
							if (x < (resolution-2)){
								otherNode = nodes[(x+1) + (y)*resolution];
								otherNode.tl = node.tr;
								otherNode.bl = node.br;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x+1)/nodesPerMesh) + Mathf.FloorToInt((y)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//bottom right
							if ((x < (resolution-2)) && (y<(resolution-2))) {
								otherNode = nodes[(x+1) + (y+1)*resolution];
								otherNode.tl = node.br;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x+1)/nodesPerMesh) + Mathf.FloorToInt((y+1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//bottom
							if (y < (resolution-2)){
								otherNode = nodes[(x) + (y+1)*resolution];
								otherNode.tl = node.bl;
								otherNode.tr = node.br;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x)/nodesPerMesh) + Mathf.FloorToInt((y+1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//bottom left
							if ((x > 1) && (y<(resolution-2))) {
								otherNode = nodes[(x-1) + (y+1)*resolution];
								otherNode.tr = node.bl;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x-1)/nodesPerMesh) + Mathf.FloorToInt((y+1)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							//left
							if (x > 1) {
								otherNode = nodes[(x-1) + (y)*resolution];
								otherNode.tr = node.tl;
								otherNode.br = node.bl;
								
								otherNode.reInit(colorValue);
								
								otherMesh = nodeMeshes[Mathf.FloorToInt((x-1)/nodesPerMesh) + Mathf.FloorToInt((y)/nodesPerMesh) * meshCount];
								otherMesh.isDirty = true;
							}
							
							node.reInit(colorValue);
							
							//nodeValuesTexture.SetPixel(x,-y, Color.white * (colorValue + (node.tl + node.tr + node.bl + node.br) * 0.25f)); 
						}
							
					}
				}
			}
		}
		
		//nodeValuesTexture.Apply();
		
	}
	
	public void clearAll() {
		
		if (nodes != null) {
			clean();
		}
		
		createTerrain(null);
		
	}
	
	public Destroy2DNode getNode(int x, int y) {
		if (nodes == null) return null;
		
		if (x >= resolution) return null;
		if (y >= resolution) return null;
		
		return nodes[x + y*resolution];
	}
	
	public float getNodeSize() {
		return terrainSize / resolution;
	}
	
	public void paintColorMask(int type, Vector3 pos, float radius, float intensity) {
		
		createColorMask();
				
		float nodeSize = terrainSize / resolution;
		int radiusSize = Mathf.FloorToInt(radius / nodeSize) + 1;
		int posX = Mathf.FloorToInt((pos.x - transform.position.x) / nodeSize);
		int posY = Mathf.FloorToInt(-(pos.y - transform.position.y) / nodeSize);
				
		for (int x = posX - radiusSize; x < posX + radiusSize; x++) {
			for (int y = posY - radiusSize; y < posY + radiusSize; y++) {
				if ((x>=0) && (x<resolution) && (y>=0) && (y<resolution)) {
					Destroy2DNode node = nodes[x + y*resolution];
					
					if (node != null) {
						float rad = ((node.nodePosition + transform.position) - pos).magnitude;
						if (rad < radius) {
							Color currentColor = colorMasks.GetPixel(x, -y);
							float factor = (1-(rad/radius)) * intensity;
							
							if (type == 0) {
								currentColor.r += factor;
								currentColor.g -= factor;
								currentColor.b -= factor;
								currentColor.a -= factor;
							}
							
							if (type == 1) {
								currentColor.r -= factor;
								currentColor.g += factor;
								currentColor.b -= factor;
								currentColor.a -= factor;
							}
							
							if (type == 2) {
								currentColor.r -= factor;
								currentColor.g -= factor;
								currentColor.b += factor;
								currentColor.a -= factor;
							}
							
							if (type == 3) {
								currentColor.r -= factor;
								currentColor.g -= factor;
								currentColor.b -= factor;
								currentColor.a += factor;
							}
							
							if (type == 4) {
								currentColor.r -= factor;
								currentColor.g -= factor;
								currentColor.b -= factor;
								currentColor.a -= factor;
							}
							
							currentColor.r = Mathf.Clamp01(currentColor.r);
							currentColor.g = Mathf.Clamp01(currentColor.g);
							currentColor.b = Mathf.Clamp01(currentColor.b);
							currentColor.a = Mathf.Clamp01(currentColor.a);
							
							
							colorMasks.SetPixel(x,-y, currentColor);			
						}
					}
				}
			}
		}
		
		colorMasks.Apply();
		
		updateMaterial();
		
	}
	
	void createColorMask() {
		if (colorMasks == null) {
			colorMasks = new Texture2D(resolution, resolution,TextureFormat.ARGB32, false);
			
			for (int x=0; x<resolution; x++)
				for (int y=0; y<resolution; y++)
					colorMasks.SetPixel(x, y, Color.red);
			
			colorMasks.Apply();
		}
	}
	
	public void rebuidMesh() {
		foreach (Destroy2DMesh mesh in GetComponentsInChildren<Destroy2DMesh>()) {
			mesh.commit(material, colliderDepth, sortingLayer, sortingOrder, this);
		}
	}
	
	public bool isInsideTerrain(Vector3 pos) {
		Vector3 dif = pos - transform.position;
		
		return (dif.x > 0) && (dif.y > 0) && (dif.x < terrainSize) && (dif.y < terrainSize);
	}
	
	public void rebuild(float newSize, int newResolution, int newNodesPerMesh, float newColliderDepth, int newSortingLayer, int newSortingOrder) {
		
		Texture2D oldValues = new Texture2D(resolution+1, resolution+1);
		
		for (int x=0; x<resolution; x++) {
			for (int y=0; y<resolution; y++) {
				Destroy2DNode node = getNode(x,resolution - y);
				if (node != null) {
					oldValues.SetPixel(x,y,		new Color(0,0,0,node.tl));
					oldValues.SetPixel(x+1,y,	new Color(0,0,0,node.tr));
					oldValues.SetPixel(x,y+1,	new Color(0,0,0,node.bl));
					oldValues.SetPixel(x+1,y+1,	new Color(0,0,0,node.br));
				} else {
					oldValues.SetPixel(x,y,		new Color(0,0,0,0));
					oldValues.SetPixel(x+1,y,	new Color(0,0,0,0));
					oldValues.SetPixel(x,y+1,	new Color(0,0,0,0));
					oldValues.SetPixel(x+1,y+1,	new Color(0,0,0,0));
				}
			}	
		}
		
		oldValues.Apply();
		
		clean();
		
		terrainSize = newSize;
		resolution = newResolution;
		nodesPerMesh = newNodesPerMesh;
		colliderDepth = newColliderDepth;
		sortingLayer = newSortingLayer;
		sortingOrder = newSortingOrder;
		
		createTerrain(oldValues);
				
		
	}
}
