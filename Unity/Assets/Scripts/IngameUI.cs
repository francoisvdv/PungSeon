using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IngameUI : MonoBehaviour
{
    private float theTimer = 0.0f;
    public float theStartTimer = 218.0f;
	public GUIStyle style;
	public GUIStyle style2;
    public GUIStyle style3;
	public Texture hpbar;
	public Texture hpbarEmpty;
    float displayMinutes;
    float displaySeconds;

    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;

    void Start()
    {
        theTimer = theStartTimer;

        switch (Options.SongIndex)
        {
            case 0:
                this.audio.clip = audio1;
                break;

            case 1:
                this.audio.clip = audio2;
                break;

            case 2:
                this.audio.clip = audio3;
                break;

            case 3:
                this.audio.clip = audio4;
                break;
        }

        this.audio.Play();
    }

    void Update()
    {
        theTimer -= Time.deltaTime;

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

        int hp = GameManager.Instance.GetPlayer().Health;
		GUI.DrawTexture(new Rect((Screen.width/2)-220, Screen.height-55, 427, 60), hpbarEmpty);
		GUI.DrawTexture(new Rect((Screen.width/2)-208, Screen.height-46, hp*4, 29), hpbar);
        GUI.Label(new Rect((Screen.width/2)-13, Screen.height-43, 100, 20), hp+"%", style2);

        List<Player> sorted = GameManager.Instance.GetPlayers();
        sorted.Sort(delegate(Player p1, Player p2) { return p2.Score.CompareTo(p1.Score); });

        int y = 20;
        foreach (var v in sorted)
        {
            Color c = GameManager.Instance.GetColor(v.PlayerIP);

            if (v.PlayerIP.Equals(Client.GetLocalIPAddress()))
                GUI.Label(new Rect(Screen.width - 250, y, 10, 10), v.PlayerIP.ToString(), style2);
            else
                GUI.Label(new Rect(Screen.width - 250, y, 10, 10), v.PlayerIP.ToString(), style3);

            GUI.Label(new Rect(Screen.width - 50, y, 10, 10), v.Score.ToString(), style2);
            GUI.DrawTexture(new Rect(Screen.width - 270, y + 6, 10, 10), listBlock(c));

            y += 21;
        }
	}

    Dictionary<Color, Texture2D> textures = new Dictionary<Color, Texture2D>();
	Texture listBlock(Color color)
    {
        if (textures.ContainsKey(color))
            return textures[color];

	    var texture = new Texture2D(10, 10, TextureFormat.ARGB32, false);
		
		for (int y = 0; y < 10; ++y) {
		    for (int x = 0; x < 10; ++x) {
		        texture.SetPixel(x, y, color);
		    }
		}
		texture.Apply();

        textures.Add(color, texture);
		return texture;
	}   
}