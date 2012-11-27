using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
	const int mainMenuWidth = 230;
	const int mainMenuHeight = 443;
	const int lobbyWidth = 460;
	const int lobbyHeight = 443;

	int boxWidth;
	int boxHeight;
	int sourceBoxWidth;
	int sourceBoxHeight;
	int targetBoxWidth;
	int targetBoxHeight;
	
	bool animating = false;
	float animationTimer;
	
	enum MenuState { MainMenu, Lobby, HowTo }
	MenuState State = MenuState.MainMenu;
	
	public float AnimationDuration = 0.2f;
	public Texture LeftTexture;
	public Texture CenterTexture;
	public Texture RightTexture;
	public GUISkin Skin;
	
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
			animationTimer += Time.deltaTime;
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
			GUI.DrawTexture(new Rect(backgroundBounds.x + leftWidth, backgroundBounds.y,
				backgroundBounds.width - leftWidth - rightWidth, backgroundBounds.height), CenterTexture);
			GUI.DrawTexture(new Rect(backgroundBounds.x + backgroundBounds.width - rightWidth, backgroundBounds.y, rightWidth,
				backgroundBounds.height), RightTexture);
		}
		GUI.BeginGroup(backgroundBounds);
		
		switch(State)
		{
		case MenuState.MainMenu:
			GuiMainMenu();
			break;
		case MenuState.Lobby:
			GuiLobby();
			break;
		case MenuState.HowTo:
			GuiHowTo();
			break;
		}
		
		GUI.EndGroup();
		GUI.skin = oldSkin;
	}
	
	void GuiMainMenu()
	{	
		int x = (boxWidth - 100) / 2;
		int y = 150;
		int width = 100;
		int height = 30;
		int gap = 10;
		
		if(GUI.Button(new Rect(x, y, width, height), "Play"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.Lobby;
		}
		if(GUI.Button(new Rect(x, y += height + gap, width, height), "How to Play"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.Lobby;
		}
		if(GUI.Button(new Rect(x, y += height + gap, width, height), "Options"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.Lobby;
		}
		if(GUI.Button(new Rect(x, y += height + gap, width, height), "High Scores"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.Lobby;
		}
		if(GUI.Button(new Rect(x, y += height + gap, width, height), "Quit"))
		{
			AnimateBackground(lobbyWidth, lobbyHeight);
			State = MenuState.Lobby;
		}
	}
	void GuiLobby()
	{
		if(GUI.Button(new Rect((boxWidth - 100) / 2, 10, 100, 30), "Back to main"))
		{
			AnimateBackground(mainMenuWidth, mainMenuHeight);
			State = MenuState.MainMenu;
		}
	}
	void GuiHowTo()
	{
	}
}
