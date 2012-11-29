using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Menu : MonoBehaviour
{
	const int mainMenuWidth = 230;
	const int mainMenuHeight = 443;
	const int lobbyListWidth = 460;
	const int lobbyListHeight = 443;
	const int createLobbyWidth = 460;
	const int createLobbyHeight = 443;
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
	public GUISkin LobbyItemSkin;

	Vector2 lobbyListScrollPosition = Vector2.zero;
	
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
		case MenuState.CreateLobby:
			GuiCreateLobby();
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
			AnimateBackground(lobbyListWidth, lobbyListHeight);
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
		int rowWidth = boxWidth - 120;
		int rowHeight = 30;
		int rowGap = 3;
		
		GuiTitle("Multiplayer Lobbies");
				
		//table box
		{
			int lobbyCount = 40;
			
			lobbyListScrollPosition = GUI.BeginScrollView(
				new Rect(50, 150, boxWidth - 100, boxHeight - 250),
				lobbyListScrollPosition,
				new Rect(0,0, rowWidth, lobbyCount * rowHeight + (lobbyCount - 1) * rowGap));
			
			int y = 0;
			for(int i = 0; i < lobbyCount; i++)
			{
				GUI.BeginGroup(new Rect(0, y, rowWidth, rowHeight));
				
				if(GUI.Button(new Rect(0, 0, rowWidth, rowHeight), "", LobbyItemSkin.button))
					OnJoinLobbyPressed();
				
				GUI.Label(new Rect(0, 0, rowWidth - 20, 30), "Yo Momma's Server");
				GUI.Label(new Rect(rowWidth - 30, 0, 30, 30), "2/6");
				
				GUI.EndGroup();
				y += rowHeight + rowGap;
			}
			
			GUI.EndScrollView();
		}
		
		if(GuiBackButton())
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
		
		if(GUI.Button(new Rect(boxWidth-180, boxHeight-80, 70, 30), "Create"))
		{
			AnimateBackground(createLobbyWidth, createLobbyHeight);
			State = MenuState.CreateLobby;
		}
	}
	void GuiCreateLobby()
	{
		GuiTitle("Create Lobby");
		
		if(GuiBackButton())
		{
			AnimateBackground(lobbyListWidth, lobbyListHeight);
			State = MenuState.LobbyList;
		}
	}
	void GuiLobby()
	{	
		GuiTitle("Lobby");

		if(GuiBackButton())
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiHowTo()
	{
		GuiTitle("How-to");
		
		if(GuiBackButton())
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiOptions()
	{
		GuiTitle("Options");
		
		if(GuiBackButton())
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiHighScores()
	{
		GuiTitle("Highscores");
		
		if(GuiBackButton())
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	
	void GuiTitle(string title)
	{
		GUI.Label(new Rect(110, 91, 300, 50), title);
	}
	bool GuiBackButton()
	{
		return GUI.Button(new Rect(100, boxHeight-80, 70, 30), "Back");
	}
	
	void OnJoinLobbyPressed()
	{
	}
	void OnCreateLobbyPressed()
	{
		print ("IK BEN BEDRUKT!");
	}
}