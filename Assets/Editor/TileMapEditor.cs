using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileMap))]
public class TileMapEditor : Editor
{
	public TileMap map;

	TileBrush brush;
	Vector3 mouse_hit_pos;

	bool mouse_on_map
	{
		get { return (mouse_hit_pos.x > 0 && mouse_hit_pos.x < map.grid_size.x
			&& mouse_hit_pos.y < 0 && mouse_hit_pos.y > -map.grid_size.x); }
	}
	public override void OnInspectorGUI()
	{
		EditorGUILayout.BeginVertical ();

		var old_size = map.map_size;
		map.map_size = EditorGUILayout.Vector2Field ("Map Size:", map.map_size);
		if (map.map_size != old_size)
			UpdateCalculations ();

		var old_text = map.texture2d;
		map.texture2d = (Texture2D)EditorGUILayout.ObjectField ("Texture2D:", map.texture2d,
			typeof(Texture2D), false);

		if (old_text != map.texture2d)
		{
			UpdateCalculations ();
			map.tile_id = 1;
			CreateBrush ();
		}

		if (map.texture2d == null)
			EditorGUILayout.HelpBox ("You have not selected a texture 2D yet.", MessageType.Warning);
		else
		{
			EditorGUILayout.LabelField ("Tile Size:", map.tile_size.x + " x " + map.tile_size.y);
			EditorGUILayout.LabelField ("Grid Size In Units:", map.grid_size.x + " x " + map.grid_size.y);
			EditorGUILayout.LabelField ("Pixels To Units:", map.pixels_to_unit.ToString());
			UpdateBrush (map.curr_tile_brush);

			if (GUILayout.Button("Clear tiles"))
			{
				if (EditorUtility.DisplayDialog ("Clear map's tiles?", "Are you sure?", "Clear", "Do not clear"))
					ClearMap ();
			}
		}

		EditorGUILayout.EndVertical ();
	}

	void OnEnable()
	{
		map = target as TileMap;
		Tools.current = Tool.View;

		if (map.tiles == null)
		{
			var go = new GameObject ("Tiles");
			go.transform.SetParent (map.transform);
			go.transform.position = Vector3.zero;

			map.tiles = go;
		}

		if (map.texture2d != null)
		{
			UpdateCalculations ();
			NewBrush ();
		}
	}

	void OnDisable()
	{
		DestroyBrush ();
	}

	void OnSceneGUI()
	{
		if (brush != null)
		{
			UpdateHitPosition ();
			MoveBrush ();

			if (map.texture2d != null && mouse_on_map)
			{
				Event current = Event.current;
				if (current.shift)
					Draw ();
				else if (current.alt)
					RemoveTile ();
			}
		}
	}

	void UpdateCalculations()
	{
		var path = AssetDatabase.GetAssetPath (map.texture2d);
		map.sprite_references = AssetDatabase.LoadAllAssetsAtPath (path);

		var sprite = (Sprite)map.sprite_references [1];
		var width = sprite.textureRect.width;
		var height = sprite.textureRect.height;

		map.tile_size = new Vector2 (width, height);
		map.pixels_to_unit = (int)(sprite.rect.width / sprite.bounds.size.x);
		map.grid_size = new Vector2 ((width / map.pixels_to_unit) * map.map_size.x,
			(height / map.pixels_to_unit) * map.map_size.y);
	}

	void CreateBrush()
	{
		var sprite = map.curr_tile_brush;
		if (sprite != null)
		{
			GameObject go = new GameObject ("Brush");

			go.transform.SetParent (map.transform);

			brush = go.AddComponent<TileBrush> ();
			brush.renderer2d = go.AddComponent<SpriteRenderer> ();
			brush.renderer2d.sortingOrder = 1000;

			var pixels_to_unit = map.pixels_to_unit;
			brush.brush_size = new Vector2 (sprite.textureRect.width / pixels_to_unit, sprite.textureRect.height / pixels_to_unit);
			brush.UpdateBrush (sprite);
		}
	}

	void NewBrush()
	{
		if (brush == null)
			CreateBrush ();
	}

	void DestroyBrush()
	{
		if (brush != null)
			DestroyImmediate (brush.gameObject);
	}

	public void UpdateBrush(Sprite sprite)
	{
		if (brush != null)
			brush.UpdateBrush (sprite);
	}

	void UpdateHitPosition()
	{
		var p = new Plane (map.transform.TransformDirection (Vector3.forward), Vector3.zero);
		var ray = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
		var hit = Vector3.zero;
		var dist = 0f;

		if (p.Raycast (ray, out dist))
			hit = ray.origin + ray.direction.normalized * dist;

		mouse_hit_pos = map.transform.InverseTransformPoint (hit);
	}

	void MoveBrush()
	{
		var tile_size = map.tile_size.x / map.pixels_to_unit;

		var x = Mathf.Floor (mouse_hit_pos.x / tile_size) * tile_size;
		var y = Mathf.Floor (mouse_hit_pos.y / tile_size) * tile_size;

		var row = x / tile_size;
		var column = Mathf.Abs (y / tile_size) - 1;

		if (!mouse_on_map)
			return; 
		var id = (int)((column * map.map_size.x) + row);

		brush.tile_id = id;

		x += map.transform.position.x + tile_size / 2;
		y += map.transform.position.y + tile_size / 2;

		brush.transform.position = new Vector3 (x, y, map.transform.position.z);
	}

	void Draw()
	{
		var id = brush.tile_id.ToString ();

		var pos_x = brush.transform.position.x;
		var pos_y = brush.transform.position.y;

		GameObject tile = GameObject.Find (map.name + "/Tiles/tile_" + id);

		if (tile == null)
		{
			tile = new GameObject ("tile_" + id);
			tile.transform.SetParent (map.tiles.transform);
			tile.transform.position = new Vector3 (pos_x, pos_y, 0);
			tile.AddComponent<SpriteRenderer> ();
		}

		tile.GetComponent<SpriteRenderer> ().sprite = brush.renderer2d.sprite;
	}

	void RemoveTile()
	{
		var id = brush.tile_id.ToString ();

		GameObject tile = GameObject.Find (map.name + "/Tiles/tile_" + id);

		if (tile != null)
			DestroyImmediate (tile);
	}

	void ClearMap()
	{
		var i = 0;

		while (i < map.tiles.transform.childCount)
		{
			Transform t = map.tiles.transform.GetChild (i);
			DestroyImmediate (t.gameObject);
		}
	}
}
