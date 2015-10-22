using System;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Destroy2DMesh : MonoBehaviour {
	
	public bool isDirty = false;
	public int meshX = 0;
	public int meshY = 0;
	public int meshNodeCount = 0;

	private float dirtyTimer = 0;

	void Update() {

		if (isDirty) dirtyTimer = 2;

		dirtyTimer -= Time.deltaTime;

		if (dirtyTimer > 0) {
			Debug.DrawLine(transform.position, transform.position + Vector3.up);
			Debug.DrawLine(transform.position, transform.position + Vector3.down);
			Debug.DrawLine(transform.position, transform.position + Vector3.left);
			Debug.DrawLine(transform.position, transform.position + Vector3.right);
		}

	}

	public void commit(Material material, float colliderDepth, int sortingLayer, int sortingOrder, Destroy2D terrain) {
		
		StartCoroutine(buildMeshCoroutine(material, colliderDepth, sortingLayer, sortingOrder, terrain));
	}
	
	public void commitEditor(Material material, float colliderDepth, int sortingLayer, int sortingOrder, Destroy2D terrain) {
		buildMesh(material, colliderDepth, sortingLayer, sortingOrder, terrain);
	}
	
	private IEnumerator buildMeshCoroutine(Material material, float colliderDepth, int sortingLayer, int sortingOrder, Destroy2D terrain) {
		
		yield return new WaitForEndOfFrame();
		buildMesh(material, colliderDepth, sortingLayer, sortingOrder, terrain);
	}
		
	private void buildMesh(Material material, float colliderDepth, int sortingLayer, int sortingOrder, Destroy2D terrain) {
		if (GetComponent<MeshCollider>() != null) {
			DestroyImmediate(GetComponent<MeshCollider>());
		}
		
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
		MeshFilter mf = gameObject.GetComponent<MeshFilter>();
		PolygonCollider2D polyCollider = gameObject.GetComponent<PolygonCollider2D>();

		Mesh m;
		
		if (mr != null) {
			DestroyImmediate(mr);
		}
		
		if (mf != null) {
			if (mf.sharedMesh != null) {
				mf.sharedMesh.Clear();
			}
			
			DestroyImmediate(mf);
		}
		
		if (terrain == null) {
			//GameObject.DestroyImmediate(gameObject);
			return;
		}

		if (polyCollider != null) {
			DestroyImmediate(polyCollider);
			polyCollider = null;
		}
		
		mr = gameObject.AddComponent<MeshRenderer>();
		mf = gameObject.AddComponent<MeshFilter>();

		m = new Mesh();
		
		mf.sharedMesh = m;
		
		int vertexCount = 0;
		int indexCount = 0;
		int uvCount = 0;
		int uv1Count = 0;
		
		int colliderCount = 0;
		
		for (int x = 0; x < meshNodeCount; x++) {
			for (int y = 0; y < meshNodeCount; y++) {
				Destroy2DNode node = terrain.getNode(x+meshX, y+meshY);
				if (node != null) { 
					if (node.isFull || node.isGeometry) {
						vertexCount 	+= node.vertices.Length;
						indexCount 		+= node.indexes.Length;
						uvCount 		+= node.uv.Length;
						uv1Count		+= node.uv1.Length;
						
						if (node.isGeometry) {
							colliderCount++;
							
							if (node.hasSecondNormal) {
								colliderCount++;
							}
						}
					}
				}
			}
		}
		
		Vector3[] vertices 	= new Vector3[vertexCount];
		Vector3[] normals   = new Vector3[vertexCount];
		Vector4[] tangents 	= new Vector4[vertexCount];
		int[] indexes		= new int[indexCount];
		Vector2[] uv		= new Vector2[uvCount];
		Vector2[] uv1		= new Vector2[uv1Count];

		vertexCount = 0;
		indexCount = 0;
		uvCount = 0;
		uv1Count = 0;
		
		colliderCount = 0;
		
		for (int x = 0; x < meshNodeCount; x++) {
			for (int y = 0; y < meshNodeCount; y++) {
				Destroy2DNode node = terrain.getNode(x+meshX, y+meshY);
				if (node != null) {
					if (node.isFull || node.isGeometry) {
						Vector3 nodePos = node.nodePosition - gameObject.transform.localPosition;
						
						for (int i=0; i<node.vertices.Length; i++) {
							vertices[vertexCount + i] = node.vertices[i] + nodePos;
							normals[vertexCount + i] = Vector3.back;
							tangents[vertexCount+i] = new Vector4(1,0,0,-1);
						}
						
						for (int i=0; i<node.indexes.Length; i++)
							indexes[indexCount + i] = node.indexes[i]+vertexCount;
						
						for (int i=0; i<node.uv.Length; i++)
							uv[uvCount + i] = node.uv[i];
						
						for (int i=0; i<node.uv1.Length; i++)
							uv1[uv1Count + i] = node.uv1[i];
						
						vertexCount 	+= node.vertices.Length;
						indexCount 		+= node.indexes.Length;
						uvCount 		+= node.uv.Length;
						uv1Count		+= node.uv1.Length;
						
						if (node.isGeometry) {

							if (polyCollider == null) {
								polyCollider = gameObject.AddComponent<PolygonCollider2D>();
							}

							node.addPolygonCollider(polyCollider);

							/*node.addToCollisionMesh(collVertices, collIndexes, collNormals, colliderCount, colliderDepth, nodePos);
							
							if (node.hasSecondNormal) {
								colliderCount++;
							}
							
							colliderCount++;*/
						}
					}
				}
			}
		}
		
		m.vertices 	= vertices;
		m.triangles = indexes;
		m.uv 		= uv;
		m.uv1 		= uv1;
		m.normals 	= normals;
		m.tangents 	= tangents;

		m.Optimize();

		renderer.material = material;

		gameObject.layer = terrain.gameObject.layer;

		isDirty = false;

	}
	
	public static Destroy2DMesh instantiate() {
		GameObject obj = new GameObject("Destroy2D Mesh");
		return obj.AddComponent<Destroy2DMesh>();
	}
}
