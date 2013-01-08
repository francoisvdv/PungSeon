using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Menu : MonoBehaviour, INetworkListener
{
    class Lobby
    {
        public Dictionary<string, bool> clients = new Dictionary<string, bool>();
    }

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

    Client c2s = Client.Create();

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

    Dictionary<int, Action<ResponsePackage>> waitForResponse = new Dictionary<int, Action<ResponsePackage>>();
    List<Action> syncQueue = new List<Action>();

    Dictionary<int, Lobby> lobbies = new Dictionary<int, Lobby>();


	// Use this for initialization
	void Start ()
	{
        c2s.AddListener(this);
		
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

        lock (syncQueue)
        {
            while (syncQueue.Count != 0)
            {
                syncQueue[0]();
                syncQueue.RemoveAt(0);
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
    void WaitForResponse(int responseId, Action<ResponsePackage> a)
    {
        GUI.enabled = false;
        if (!waitForResponse.ContainsKey(responseId))
            waitForResponse.Add(responseId, a);
    }
    void ResponseReceived(int responseId, ResponsePackage rp)
    {
        GUI.enabled = true;
        if(waitForResponse.ContainsKey(responseId))
            waitForResponse[responseId](rp);
    }

    void ConnectToServer()
    {
        if (c2s.GetConnectionCount() == 0)
            c2s.Connect("192.168.1.69");
    }
    void RequestLobbyList(Action onReceive)
    {
        c2s.WriteAll(new RequestLobbyListPackage());
        WaitForResponse(RequestLobbyListPackage.factory.Id,
        x =>
        {
            lobbies.Clear();

            string body = x.ResponseMessage;
            const char lobbySeperator = '|';
            const char lobbyEntrySeperator = ';';

            string[] newLobbies = body.Split(lobbySeperator);
            foreach (string l in newLobbies)
            {
                Lobby newLobby = new Lobby();
                string[] entries = l.Split(lobbyEntrySeperator);
                if (entries.Length == 0)
                    continue;

                int lobbyId;
                if (!int.TryParse(entries[0], out lobbyId))
                    continue;

                for (int i = 1; i < entries.Length; i += 2)
                {
                    bool ready;
                    if (!bool.TryParse(entries[i + 1], out ready))
                        break;
                    newLobby.clients.Add(entries[i], ready);
                }

                lobbies.Add(lobbyId, newLobby);
            }

            if(onReceive != null)
                onReceive();
        });
    }
    void CreateLobby(Action onReceive)
    {
        c2s.WriteAll(new CreateLobbyPackage());
        WaitForResponse(CreateLobbyPackage.factory.Id,
        x =>
        {
            if (onReceive != null)
                onReceive();
        });
    }
    void JoinLobby()
    {

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
            OnPlayPressed();
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
			for(int i = 0; i < lobbies.Count; i++)
			{
                Lobby l = lobbies[i];

				GUI.BeginGroup(new Rect(0, y, rowWidth, rowHeight));
				
				if(GUI.Button(new Rect(0, 0, rowWidth, rowHeight), "", LobbyItemSkin.button))
					OnJoinLobbyPressed();
				
				GUI.Label(new Rect(0, 0, rowWidth - 20, 30), i.ToString());
				GUI.Label(new Rect(rowWidth - 30, 0, 30, 30), l.clients.Count.ToString() + "/6");
				
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
            OnCreateLobbyPressed();
			//AnimateBackground(createLobbyWidth, createLobbyHeight);
			///State = MenuState.CreateLobby;
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

    void OnPlayPressed()
    {
        ConnectToServer();

        RequestLobbyList(() =>
            {
                AnimateBackground(lobbyListWidth, lobbyListHeight);
                State = MenuState.LobbyList;
            });
    }
	void OnCreateLobbyPressed()
	{
        CreateLobby(() =>
            {
                RequestLobbyList(null);
            });
	}
    void OnJoinLobbyPressed()
    {
    }

    public void OnDataReceived(DataPackage dp)
    {
        ResponsePackage rp = dp as ResponsePackage;
        if (rp == null)
            return;

        lock (syncQueue)
        {
            syncQueue.Add(() =>
            {
				ResponseReceived(rp.ResponseId, rp);
				waitForResponse.Remove(rp.ResponseId);
            });
        }
    }
}