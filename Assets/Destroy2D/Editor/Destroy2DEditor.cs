using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Destroy2D))]
class Destroy2DEditor : Editor {
	
	private int toolBarSelection = 0;
	private int toolBarSelectionPaint = 0;
	private bool drawBrush = false;
	private bool eraseBrush = false;
	private bool paint1 = false;
	private bool paint2 = false;
	private bool paint3 = false;
	private bool paint4 = false;
	private bool paintErase = false;
	//private float brushSize = 10;
	//private float brushIntensity = 0.5f;
	private bool canDraw = false;
	
	private Material materialEditor;
	private float terrainSizeEditor;
	private int resolutionEditor;
	private int nodesPerMeshEditor;
	private float colliderDepthEditor;
	private int sortingLayerEditor;
	private int sortingOrderEditor;
	
	private void drawBox() {
		GUILayout.Space(10);
		GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
	}
	
	void OnEnable() {
		Destroy2D terrain = (target as Destroy2D);
		
		materialEditor = terrain.material;
		terrainSizeEditor = terrain.terrainSize;
		resolutionEditor = terrain.resolution;
		nodesPerMeshEditor = terrain.nodesPerMesh;
		colliderDepthEditor = terrain.colliderDepth;
		sortingLayerEditor = terrain.sortingLayer;
		sortingOrderEditor = terrain.sortingOrder;

	}

	private string[] getSortingLayers() {
		List<string> result = new List<string>();

		for (int i=0; i<32; i++) {

			string layerName = LayerMask.LayerToName(i);

			if (!"".Equals(layerName)) {
				result.Add(layerName);
			}
		}

		return result.ToArray();
	}
	
	public override void OnInspectorGUI() {
		
		Destroy2D terrain = (target as Destroy2D);
		
		drawBox();
		
		EditorGUILayout.LabelField("Commands");
		
		GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Clear All")) {
				terrain.clearAll();
			}
		
		GUILayout.EndHorizontal();
		
		drawBox();
		
		EditorGUILayout.LabelField("Options");
		EditorGUI.indentLevel++;
		
			
			EditorGUIUtility.LookLikeInspector();
			materialEditor 		= EditorGUILayout.ObjectField("Material", materialEditor, typeof(Material), false) as Material;
			terrainSizeEditor 	= EditorGUILayout.FloatField("Size", terrainSizeEditor);
			resolutionEditor 	= EditorGUILayout.IntField("Resolution", resolutionEditor);
			nodesPerMeshEditor 	= EditorGUILayout.IntField("Nodes Per Mesh", nodesPerMeshEditor);
			//colliderDepthEditor = EditorGUILayout.FloatField("Collider Depth", colliderDepthEditor);
			//sortingLayerEditor	= EditorGUILayout.IntField("Sorting Layer", sortingLayerEditor);	
			//sortingOrderEditor	= EditorGUILayout.IntField("Sorting Order", sortingOrderEditor);	

		
			if (terrain.nodesPerMesh < 1) terrain.nodesPerMesh = 1;
			if (terrain.nodesPerMesh > terrain.resolution) terrain.nodesPerMesh = terrain.resolution;
			
		EditorGUI.indentLevel--;
		
		if (materialEditor != terrain.material) {
			terrain.material = materialEditor;
			terrain.updateMaterial();
		}
		
		if ((terrainSizeEditor != terrain.terrainSize) ||
			(resolutionEditor != terrain.resolution) ||
			(nodesPerMeshEditor != terrain.nodesPerMesh) ||
			(colliderDepthEditor != terrain.colliderDepth) || 
		    (sortingLayerEditor != terrain.sortingLayer) ||
		    (sortingOrderEditor != terrain.sortingOrder)) {
			
			if (GUILayout.Button("Commit")) {
				terrain.rebuild(terrainSizeEditor, resolutionEditor, nodesPerMeshEditor, colliderDepthEditor, sortingLayerEditor, sortingOrderEditor);
			}
			
		}
		
		
		drawBox();
		
		EditorGUILayout.LabelField("Paint");
		EditorGUI.indentLevel++;
		
		terrain.brushIntensity = EditorGUILayout.Slider("Brush Intensity", terrain.brushIntensity, 0.1f, 1.0f);
		terrain.brushSize = EditorGUILayout.Slider("Brush Size", terrain.brushSize, terrain.getNodeSize(), terrain.terrainSize);
		
		
		string[] toolBarOptions = {"Add Terrain", "Remove Terrain", "Paint"};
		toolBarSelection = GUILayout.Toolbar(toolBarSelection, toolBarOptions);
		
		drawBrush = (toolBarSelection == 0);
		eraseBrush = (toolBarSelection == 1);
		
		paint1 = paint2 = paint3 = paint4 = false;
		
		if (toolBarSelection == 2) {
			
			GUILayout.Label("Paint Options");
			
			string[] toolBarOptionsPaint = {"Mask 1", "Mask 2", "Mask 3", "Mask 4", "Erase"};
			//toolBarSelectionPaint = GUILayout.SelectionGrid(toolBarSelectionPaint, toolBarOptionsPaint, 5, new GUILayoutOption[]{GUILayout.Width(buttonWidth), GUILayout.Height(buttonWidth/5)});
			
			toolBarSelectionPaint = GUILayout.Toolbar(toolBarSelectionPaint, toolBarOptionsPaint);
			
			paint1 = (toolBarSelectionPaint == 0);
			paint2 = (toolBarSelectionPaint == 1);
			paint3 = (toolBarSelectionPaint == 2);
			paint4 = (toolBarSelectionPaint == 3);
			paintErase = (toolBarSelectionPaint == 4); 
			
		}
		
	}
	
	void OnSceneGUI() {
		
		canDraw = !(((GameObject.Find( "SceneCamera" ).camera.pixelWidth - Event.current.mousePosition.x) < 80) &&
				  (Event.current.mousePosition.y < 110));
		
		if (canDraw) {
			if (drawBrush) drawTerrain(-1);
			if (eraseBrush) drawTerrain(1);
			if (paint1) paintTerrain(0);
			if (paint2) paintTerrain(1);
			if (paint3) paintTerrain(2);
			if (paint4) paintTerrain(3);
			if (paintErase) paintTerrain(4);
		}
		
		
	}
	
	private Vector3 drawBrushGizmo() {
		
		Vector2 mousePos = new Vector2(Event.current.mousePosition.x, GameObject.Find( "SceneCamera" ).camera.pixelHeight -  Event.current.mousePosition.y);
		
		Destroy2D terrain = (target as Destroy2D);
		
		if (Event.current.type == EventType.layout)
        	HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
		
		Ray mouseRay = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos);// Camera.current.ScreenPointToRay(Event.current.mousePosition);
		
		Plane p = new Plane(Vector3.back, terrain.transform.position.z);
		
		float distance = 0;
			
		p.Raycast(mouseRay, out distance);
		
		mouseRay.direction = new Vector3(mouseRay.direction.x, mouseRay.direction.y, mouseRay.direction.z);
		
		Vector3 planePos = mouseRay.origin + mouseRay.direction * distance;
		
		if (planePos.x < terrain.transform.position.x) planePos.x = terrain.transform.position.x;
		if (planePos.x > (terrain.transform.position.x + terrain.terrainSize)) planePos.x = (terrain.transform.position.x + terrain.terrainSize);
		if (planePos.y > terrain.transform.position.y) planePos.y = terrain.transform.position.y;
		if (planePos.y < (terrain.transform.position.y - terrain.terrainSize)) planePos.y = (terrain.transform.position.y - terrain.terrainSize);
		
		for (float c=0; c<10; c++) {
			Handles.color = Color.Lerp(Color.white, Color.black, c/10); 
			for (int i=0; i<20; i++) {
				Vector3 pos1 = planePos;
				Vector3 pos2 = planePos;
				
				float delta = Mathf.PI * 2 / 20;
				float radius = terrain.brushSize + c/20;
				
				pos1.x += Mathf.Cos(i*delta)*radius;
				pos1.y += Mathf.Sin(i*delta)*radius;
				
				pos2.x += Mathf.Cos((i+1)*delta)*radius;
				pos2.y += Mathf.Sin((i+1)*delta)*radius;
				
				//Debug.DrawLine(pos1, pos2);
				//Gizmos.DrawLine(pos1, pos2);
				Handles.DrawLine(pos1, pos2); 
			}
		} 
		
		
		if ((Event.current.shift) && (Event.current.type.Equals(EventType.scrollWheel))){
			
			terrain.brushSize += (Event.current.delta.x + Event.current.delta.y) * 0.1f;
			
			if (terrain.brushSize < terrain.getNodeSize()) terrain.brushSize = terrain.getNodeSize();
			
			Event.current.Use();
		}
		
		return planePos;
	}
	
	private void drawTerrain(float val) {
		
		Destroy2D terrain = (target as Destroy2D);
		
		if (terrain.brushSize < terrain.getNodeSize()) terrain.brushSize = terrain.getNodeSize();
		
		HandleUtility.Repaint();
		Vector3 planePos = drawBrushGizmo();
		
		if (Event.current.button == 0) {
			if (Event.current.type == EventType.mouseDown) {
				Undo.RegisterSceneUndo("terrain draw");
			}
			
			if (Event.current.type == EventType.mouseDrag) {
				terrain.destroyAtEditor(planePos, terrain.brushSize, terrain.brushIntensity * 0.1f * val);
				Event.current.Use();
				
			}
		} 
	}
	
	private void paintTerrain(byte paintType) {
		Destroy2D terrain = (target as Destroy2D);
		
		if (terrain.brushSize < terrain.getNodeSize()) terrain.brushSize = terrain.getNodeSize();
		
		HandleUtility.Repaint();
		Vector3 planePos = drawBrushGizmo();
		
		
		if (Event.current.button == 0) {
			if (Event.current.type == EventType.mouseDown) {
				Undo.RegisterSceneUndo("terrain paint");
			}
			
			if (Event.current.type == EventType.mouseDrag) {
						
				terrain.paintColorMask(paintType, planePos, terrain.brushSize, terrain.brushIntensity*0.1f);
				
				Event.current.Use();
				
			}
		} 
	}
	
	[MenuItem ("GameObject/Create Other/Destroy2D Object")]
	static void createDestroy2DObject() {
		
		GameObject newObj = new GameObject("Destroy2D Object");
		Destroy2D terrain = newObj.AddComponent<Destroy2D>();
		terrain.rebuild(50, 50, 5, 5, 0, 0);
		
		Vector3 planePos = Vector3.zero;
		
		if (GameObject.Find("SceneCamera") != null) {
			Vector2 mousePos = new Vector2(GameObject.Find( "SceneCamera" ).camera.pixelWidth / 2,  GameObject.Find( "SceneCamera" ).camera.pixelHeight / 2);
			
			Ray mouseRay = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mousePos);
			
			Plane p = new Plane(Vector3.back, 0);
			
			float distance = 0;
				
			p.Raycast(mouseRay, out distance);
			
			mouseRay.direction = new Vector3(mouseRay.direction.x, mouseRay.direction.y, mouseRay.direction.z);
		
			planePos = mouseRay.origin + mouseRay.direction * distance - new Vector3(0.5f, -0.5f, 0) * terrain.terrainSize;
		}
		
		
		newObj.transform.position = planePos;
		
		
		
	}
}
