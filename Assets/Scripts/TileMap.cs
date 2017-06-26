using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMap : MonoBehaviour
{
	public Vector2 map_size = new Vector2(20, 10);
	public Texture2D texture2d;
	public Vector2 tile_size = new Vector2 ();
	public Object[] sprite_references;
	public Vector2 grid_size = new Vector2();
	public int pixels_to_unit = 100;
	public int tile_id = 0;
	public GameObject tiles;

	public Sprite curr_tile_brush
	{
		get { return (sprite_references [tile_id] as Sprite); }
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnDrawGizmosSelected()
	{
		var pos = transform.position;

		if (texture2d != null)
		{
			Gizmos.color = Color.gray;
			var row = 0;
			var max_columns = map_size.x;
			var total = map_size.x * map_size.y;
			var tile = new Vector3 (tile_size.x / pixels_to_unit, tile_size.y / pixels_to_unit, 0);
			var offset = new Vector2 (tile.x / 2, tile.y / 2);
			var i = 0;

			while (i < total)
			{
				var column = i % max_columns;

				var new_x = (column * tile.x) + offset.x + pos.x;
				var new_y = -(row * tile.y) - offset.y + pos.y;

				Gizmos.DrawWireCube (new Vector2 (new_x, new_y), tile);

				if (column == max_columns - 1)
					++row;
				++i;
 			}

			Gizmos.color = Color.white;
			var center_x = pos.x + (grid_size.x / 2);
			var center_y = pos.y - (grid_size.y / 2);

			Gizmos.DrawWireCube (new Vector2 (center_x, center_y), grid_size);
		}
	}
}
