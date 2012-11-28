using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
	const int mainMenuWidth = 230;
	const int mainMenuHeight = 443;
	const int lobbyWidth = 460;
	const int lobbyHeight = 443;
	const int howToWidth = 460;
	const int howToHeight = 443;
	const int highScoresWidth = 460;
	const int highScoresHeight = 443;
	
	int boxWidth;
	int boxHeight;
	int sourceBoxWidth;
	int sourceBoxHeight;
	int targetBoxWidth;
	int targetBoxHeight;
	
	bool animating = false;
	float animationTimer;
	
	enum MenuState { MainMenu, LobbyList, CreateLobby, Lobby, HowTo, Options, HighScores }
	MenuState State = MenuState.MainMenu;
	
	public float AnimationDuration = 0.2f;
	public Texture LeftTexture;
	public Texture CenterTexture;
	public Texture RightTexture;
	public GUISkin Skin;
	public Texture TableBackground;
	
	// Use this for initialization
	void Start ()
	{
		boxWidth = mainMenuWidth;
		boxHeight = mainMenuHeight;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(animating)
		{
			animationTimer += Time.fixedDeltaTime;
			if(animationTimer > AnimationDuration)
			{
				animating = false;
				animationTimer = 0;
				boxWidth = targetBoxWidth;
				boxHeight = targetBoxHeight;
			}
			else
			{
				boxWidth = Mathf.RoundToInt(Mathf.SmoothStep(sourceBoxWidth, targetBoxWidth, animationTimer / AnimationDuration));
				boxHeight = Mathf.RoundToInt(Mathf.SmoothStep(sourceBoxHeight, targetBoxHeight, animationTimer / AnimationDuration));
			}
		}
	}
	
	void AnimateBackground(int targetWidth, int targetHeight)
	{
		if(animating)
			return;
		
		animating = true;
		animationTimer = 0;
		sourceBoxWidth = boxWidth;
		sourceBoxHeight = boxHeight;
		
		targetBoxWidth = targetWidth;
		targetBoxHeight = targetHeight;
	}
	
	void OnGUI()
	{		
		GUISkin oldSkin = GUI.skin;
		GUI.skin = Skin;
		
		Rect backgroundBounds = new Rect((Screen.width - boxWidth) / 2, (Screen.height - boxHeight) / 2, boxWidth, boxHeight);
		
		if(LeftTexture != null && CenterTexture != null && RightTexture != null)
		{
			float ratio = (float)boxHeight / LeftTexture.height;
			int leftWidth = Mathf.RoundToInt(ratio * LeftTexture.width);
			int rightWidth = Mathf.RoundToInt(ratio * RightTexture.width);
			
			GUI.DrawTexture(new Rect(backgroundBounds.x, backgroundBounds.y, leftWidth, backgroundBounds.height), LeftTexture);
			
			//GUI.DrawTexture(new Rect(backgroundBounds.x + leftWidth, backgroundBounds.y,
			//	backgroundBounds.width - leftWidth - rightWidth, backgroundBounds.height), CenterTexture,ScaleMode.StretchToFill);
			Rect centerBounds = new Rect(backgroundBounds.x + leftWidth, backgroundBounds.y, 
				backgroundBounds.width - leftWidth - rightWidth, backgroundBounds.height);
			DrawTiled (centerBounds, CenterTexture);
			
			GUI.DrawTexture(new Rect(backgroundBounds.x + backgroundBounds.width - rightWidth, backgroundBounds.y, rightWidth,
				backgroundBounds.height), RightTexture);
		}
		
		if(animating)
			GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, Mathf.SmoothStep (0, 1, animationTimer / AnimationDuration));
		else
			GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
		
		GUI.BeginGroup(backgroundBounds);
		
		switch(State)
		{
		case MenuState.MainMenu:
			GuiMainMenu();
			break;
		case MenuState.LobbyList:
			GuiLobbyList();
			break;
		case MenuState.Lobby:
			GuiLobby();
			break;
		case MenuState.HowTo:
			GuiHowTo();
			break;
		case MenuState.Options:
			GuiOptions();
			break;
		case MenuState.HighScores:
			GuiHighScores();
			break;
		}
		
		GUI.EndGroup();
		GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
		
		GUI.skin = oldSkin;
	}
	void DrawTiled (Rect rect, Texture tex)
	{
	    GUI.BeginGroup(rect);
	    {
	        int width = Mathf.RoundToInt(rect.width);
	        int height = Mathf.RoundToInt(rect.height);

            for (int x = 0; x < width; x += tex.width)
            {
                GUI.DrawTexture(new Rect(x, 0, tex.width, height), tex);
            }
	    }
	    GUI.EndGroup();
	}
	void GuiMainMenu()
	{	
		int buttonWidth = 140;
		int buttonHeight = 30;
		int x = (boxWidth - buttonWidth) / 2;
		int y = 150;
		int gap = 10;
		
		if(GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "Play"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.LobbyList;
		}
		if(GUI.Button(new Rect(x, y += buttonHeight + gap, buttonWidth, buttonHeight), "How to Play"))
		{
			AnimateBackground(howToWidth, howToHeight);
			State = MenuState.HowTo;
		}
		if(GUI.Button(new Rect(x, y += buttonHeight + gap, buttonWidth, buttonHeight), "Options"))
		{
			AnimateBackground(howToWidth, howToHeight);
			State = MenuState.Options;
		}
		if(GUI.Button(new Rect(x, y += buttonHeight + gap, buttonWidth, buttonHeight), "High Scores"))
		{
			AnimateBackground(highScoresWidth, highScoresHeight);
			State = MenuState.HighScores;
		}
		if(GUI.Button(new Rect(x, y += buttonHeight + gap, buttonWidth, buttonHeight), "Quit"))
		{
			GUIUtility.ExitGUI();
		}
	}
	void GuiLobbyList()
	{
		if(GUI.Button(new Rect(0,0,200,30), "Create match"))
		{
			OnCreateMatchPressed();
		}
	}
	void GuiCreateLobby()
	{
	}
	void GuiLobby()
	{
		int rowWidth = boxWidth - 40;
		int rowHeight = 20;
		int rowX = 0;
		
		//box
		{
	//		GUI.BeginGroup(new Rect(20, 150, rowWidth, (int)(boxHeight / (float)rowHeight)));
		}
		
		//Title
		GUI.Label(new Rect(110, 91, 300, 50), "Multiplayer lobby");
		
		//Lobby Table
		GUI.DrawTexture(new Rect(50,140,360,210), TableBackground);
		
		if(GUI.Button(new Rect(100, boxHeight-80, 70, 30), "Back"))
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
		
		if(GUI.Button(new Rect((boxWidth-180), boxHeight-80, 80, 30), "Join"))
		{
		}
	}
	void GuiHowTo()
	{
		//Title
		GUI.Label(new Rect(110, 91, 300, 50), "How-to");
		
		if(GUI.Button(new Rect(100, boxHeight-80, 70, 30), "Back"))
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiOptions()
	{
		//Title
		GUI.Label(new Rect(110, 91, 300, 50), "Options");
		
		if(GUI.Button(new Rect(100, boxHeight-80, 70, 30), "Back"))
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiHighScores()
	{
		//Title
		GUI.Label(new Rect(110, 91, 300, 50), "Highscores");
		
		if(GUI.Button(new Rect(100, boxHeight-80, 70, 30), "Back"))
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	
	void OnCreateMatchPressed()
	{
		print ("IK BEN BEDRUKT!");
	}
}
