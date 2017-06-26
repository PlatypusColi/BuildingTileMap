using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TilePickerWindow : EditorWindow
{
	public enum Scale
	{
		x1,
		x2,
		x3,
		x4,
		x5
	}

	Scale scale;
	Vector2 curr_selection = Vector2.zero;

	public Vector2 scroll_pos = Vector2.zero;

	[MenuItem("Window/Tile Picker")]
	public static void OpenTilePickerWindow()
	{
		var window = EditorWindow.GetWindow (typeof(TilePickerWindow));
		var title = new GUIContent ();
		title.text = "Tile Picker";
		window.titleContent = title;
	}

	void OnGUI()
	{
		if (Selection.activeGameObject == null)
			return;

		var selection = Selection.activeGameObject.GetComponent<TileMap> ();

		if (selection != null)
		{
			var texture2d = selection.texture2d;
			if (texture2d != null)
			{
				scale = (Scale)EditorGUILayout.EnumPopup ("Zoom", scale);
				var new_scale = ((int)scale) + 1;
				var new_text_size = new Vector2 (texture2d.width, texture2d.height) * new_scale;
				var offset = new Vector2 (10, 25);

				var view_port = new Rect (0, 0, position.width - 5, position.height - 5);
				var content_size = new Rect (0, 0, new_text_size.x + offset.x, new_text_size.y + offset.y);
				scroll_pos = GUI.BeginScrollView (view_port, scroll_pos, content_size);
				GUI.DrawTexture (new Rect (offset.x, offset.y, new_text_size.x, new_text_size.y), texture2d);

				var tile = selection.tile_size * new_scale;
				var grid = new Vector2 (new_text_size.x / tile.x, new_text_size.y / tile.y);

				var selection_pos = new Vector2 (tile.x * curr_selection.x + offset.x,
					                    tile.y * curr_selection.y + offset.y);

				var box_tex = new Texture2D (1, 1);
				box_tex.SetPixel (0, 0, new Color (0, 0.5f, 1f, 0.4f));
				box_tex.Apply ();

				var style = new GUIStyle (GUI.skin.customStyles [0]);
				style.normal.background = box_tex;

				GUI.Box (new Rect (selection_pos.x, selection_pos.y, tile.x, tile.y), "",style);

				var curr_event = Event.current;
				Vector2 mouse_pos = new Vector2 (curr_event.mousePosition.x, curr_event.mousePosition.y);
				if (curr_event.type == EventType.mouseDown && curr_event.button == 0)
				{
					curr_selection.x = Mathf.Floor ((mouse_pos.x + scroll_pos.x) / tile.x);
					curr_selection.y = Mathf.Floor ((mouse_pos.y + scroll_pos.y) / tile.y);

					if (curr_selection.x > grid.x - 1)
						curr_selection.x = grid.x - 1;
					if (curr_selection.y > grid.y - 1)
						curr_selection.y = grid.y - 1;

					selection.tile_id = (int)(curr_selection.x + (curr_selection.y * grid.x) + 1);

					Repaint ();
				}

				GUI.EndScrollView ();
			}
		}
	}
}
