using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBrush : MonoBehaviour
{
	public Vector2 brush_size = Vector2.zero;
	public int tile_id = 0;
	public SpriteRenderer renderer2d;

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube (transform.position, brush_size);
	}

	public void UpdateBrush(Sprite sprite)
	{
		renderer2d.sprite = sprite;

	}
}
