// Alan Zucconi: http://www.alanzucconi.com/?p=4643
using UnityEngine;

public class DrawMouse : MonoBehaviour {

    public RenderTexture renderTexture;

    public Texture brush0;
    public Texture brush1;
    public int size = 50;

	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Texture texture = Input.GetMouseButton(0) ? brush0 : brush1;

            Vector3 mouse = Input.mousePosition;
            mouse.z = transform.position.z;
            Vector2 screen = Camera.main.ScreenToWorldPoint(mouse);

            Debug.DrawLine(Vector3.zero, screen);

            // [-0.5,+0.5] to [0, 255]
            Vector2 pixels = new Vector2
            (   (screen.x + 0.5f) * renderTexture.width,
                renderTexture.height - (screen.y + 0.5f) * renderTexture.height
            );


            // Draw the hot or cold spot
            RenderTexture.active = renderTexture;
            Graphics.DrawTexture
            (   new Rect(pixels.x, pixels.y, size, size),
                texture
            );
            RenderTexture.active = null;
        }

    }
}
