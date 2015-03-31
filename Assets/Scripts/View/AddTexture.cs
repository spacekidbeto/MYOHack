using UnityEngine;
using System.Collections;

public class AddTexture : MonoBehaviour {

	SpriteRenderer rend;
	// Use this for initialization
	void Start () {
		this.rend = (SpriteRenderer)this.gameObject.GetComponent(typeof(SpriteRenderer));
		this.UpdateGraphics();
	}

	void UpdateGraphics() {
		Sprite mySprite = this.rend.sprite;
		int width = mySprite.texture.width;
		Texture2D oldTex = mySprite.texture;
		Color[] colors = oldTex.GetPixels();
		Texture2D tex = new Texture2D(oldTex.width, oldTex.height, TextureFormat.RGBA32, false);
		tex.SetPixels(colors);
		for (int z=0;z<width;z++) {
			for (int y=10;y<250;y++) {
				tex.SetPixel (z,y,new Color(0.5f,0.5f,0.5f,0.1f));
			}
		}
		tex.Apply();
		Vector2 pivotPoint = new Vector2(0.5f,0.5f);
		Sprite newSprite = Sprite.Create(tex, mySprite.rect, pivotPoint);
		this.rend.sprite = newSprite;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
