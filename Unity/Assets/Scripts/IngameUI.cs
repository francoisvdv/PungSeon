using UnityEngine;
using System.Collections;

public class IngameUI : MonoBehaviour
{
	public int hp = 100;
    private float theTimer = 0.0f;
    public float theStartTimer = 218.0f;
	public GUIStyle style;
	public GUIStyle style2;
	public Texture hpbar;
	public Texture hpbarEmpty;
    float displayMinutes;
    float displaySeconds;

    void Start()
    {
        theTimer = theStartTimer;
    }

    void Update()
    {
        theTimer -= Time.deltaTime;
		updateHealth(1); 
		
        if (theTimer <= 0)
        {
            Debug.Log("End game");
            theTimer = 1;
			Application.Quit();
        }
    }

    void OnGUI()
    {
        displayMinutes = Mathf.CeilToInt(theTimer-30) / 60.0f;
        displaySeconds = Mathf.CeilToInt(theTimer) % 60.0f;
		
        string text = string.Format("{0:00}:{1:00}", displayMinutes.ToString("0"), displaySeconds.ToString("00"));
		GUI.Label (new Rect(10, 6, 100, 30),"Time remaining:");
        GUI.Label(new Rect(10, 20, 100, 20), text, style);
		
		GUI.DrawTexture(new Rect((Screen.width/2)-220, Screen.height-55, 427, 60), hpbarEmpty);
		GUI.DrawTexture(new Rect((Screen.width/2)-208, Screen.height-46, hp*4, 29), hpbar);
        GUI.Label(new Rect((Screen.width/2)-13, Screen.height-43, 100, 20), hp+"%", style2);

        int y = 20;
        foreach (var v in GameManager.Instance.GetPlayers())
        {
            Color c = GameManager.Instance.GetColor(v.PlayerIP);

            GUI.Label(new Rect(Screen.width - 250, y, 10, 10), v.PlayerIP.ToString(), style2);
            GUI.Label(new Rect(Screen.width - 50, y, 10, 10), v.Score.ToString(), style2);
            GUI.DrawTexture(new Rect(Screen.width - 270, y + 6, 10, 10), listBlock(c));

            y += 21;
        }
	}
	
	void updateHealth(int damage)
    {
		if (hp>0) {
	        hp -= damage;
		}
	}
	
	Texture listBlock(Color color)
    {
	    var texture = new Texture2D(10, 10, TextureFormat.ARGB32, false);
		
		for (int y = 0; y < 10; ++y) {
		    for (int x = 0; x < 10; ++x) {
		        texture.SetPixel(x, y, color);
		    }
		}
		texture.Apply();
		return texture;
	}   
}