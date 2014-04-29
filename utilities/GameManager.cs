/**
 * \file GameManager.cs
 *
 * \brief Implements the GameManager class.
 */

using System;
using System.Collections.Generic;

using Sce.PlayStation.Core;
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
using Sce.PlayStation.Core.Environment;
using Sce.PlayStation.Core.Input;

using API.Framework;
using API;

namespace SpaceShips
{
	public class GameManager : GameActor
	{
		public UInt32 cnt = 0;
		private int cursor = 0;
		private bool analogBlock = true;
		private bool arcade = true;

		public Dictionary<string, SimpleSprite> gameManagerSprites;

		/**
		 * \fn GameManager(Game gs, string name)
		 *
		 * \brief Default constructor.
		 *
		 * Creates the sprites of each texture and set to the start position.
		 *
		 * \param gs		Variable that contains the game framework class.
 		 * \param name 		Name of the object.
 		 */
		public GameManager(Game gs, string name) : base (gs, name)
		{
			SimpleSprite sprite;

			gameManagerSprites = new Dictionary<string, SimpleSprite>();

			sprite = new SimpleSprite(gs.Graphics, gs.textureMenu, 0, 38, gs.textureMenu.Width,75,
			                          gs.textureMenu.Width, 37, 90.0f,
			                          new Vector2(gs.rectScreen.Width/2-37*2,
			            gs.rectScreen.Height/2+gs.textureMenu.Width/2));
			gameManagerSprites.Add("title", sprite);

			sprite = new SimpleSprite(gs.Graphics, gs.textureMenu, 0,0,gs.textureMenu.Width,37,
			                          gs.textureMenu.Width, 37, 90.0f,
			                          new Vector2(gameManagerSprites["title"].Position.X+128,
			            gameManagerSprites["title"].Position.Y));
			gameManagerSprites.Add("pressStart", sprite);

			sprite = new SimpleSprite(gs.Graphics, gs.textureMenu, 0,gs.textureMenu.Height-37,
			                             gs.textureMenu.Width,gs.textureMenu.Height,
			                          gs.textureMenu.Width, 37, 90.0f,
			                          new Vector2(gameManagerSprites["title"].Position.X+128,
			            gameManagerSprites["title"].Position.Y));
			gameManagerSprites.Add("arcade", sprite);

			sprite = new SimpleSprite(gs.Graphics, gs.textureMenu, 0,114,gs.textureMenu.Width,151,
			                          gs.textureMenu.Width, 37, 90.0f,
			                          new Vector2(gameManagerSprites["arcade"].Position.X+64,
			            gameManagerSprites["title"].Position.Y));
			gameManagerSprites.Add("survival", sprite);

			sprite = new SimpleSprite(gs.Graphics, gs.textureMenu, 0,76,gs.textureMenu.Width,113,
			                          gs.textureMenu.Width, 37, 90.0f,
			                          new Vector2(gs.rectScreen.Width/2-37*2,
			            gs.rectScreen.Height/2+235/2));
			gameManagerSprites.Add("gameover", sprite);
		}

		/**
		 * \fn Update()
		 *
		 * \brief Overrided update method.
		 *
		 * Handle the differents game states.
		 */
		public override void Update()
		{
#if DEBUG
//			gs.debugStringScore.Clear();
//			gs.debugStringShip.Clear();
//
//			string str = String.Format("Score: {0:d8}", gs.Score);
//			gs.debugStringScore.SetPosition(new Vector3( gs.rectScreen.Width/2-(str.Length*10/2), 2, 0));
//			gs.debugStringScore.WriteLine(str);
//
//			str = String.Format("Ships:{0}", gs.NumShips);
//			gs.debugStringShip.SetPosition(new Vector3( gs.rectScreen.Width-(str.Length*10), 2, 0));
//			gs.debugStringShip.WriteLine(str);
#endif
			switch(gs.Step)
			{
			case Game.StepType.Opening:
				break;
			case Game.StepType.Title:
				if (gs.playerInput.StartButton())
				{
					gs.Step = Game.StepType.StartMenu;
					gs.audioManager.playSound("systemSelect");

				}
				break;
			case Game.StepType.StartMenu:
#if DEBUG
				gs.debugString.WriteLine("StartMenu");
#endif
				// Cursor handler
				if (gs.playerInput.LeftRightAxis() < 0.0f && analogBlock)
				{
					analogBlock = false;
					gs.audioManager.playSound("systemSelect");
					if (cursor==0)
						cursor = 1;
					else
						cursor = 0;
			    }
				else if (gs.playerInput.LeftRightAxis() > 0.0f && analogBlock)
				{
					analogBlock = false;
					gs.audioManager.playSound("systemSelect");
					if (cursor==1)
						cursor = 0;
					else
						cursor = 1;
				}
				if (gs.playerInput.LeftRightAxis() == 0.0f)
					analogBlock = true;

				// Menu handler
				switch (cursor)
				{
					case 0:
						gameManagerSprites["arcade"].SetColor(1.0f, 1.0f, 1.0f, 0.75f);
						gameManagerSprites["survival"].SetColor(1.0f, 1.0f, 1.0f, 1.0f);
						if (gs.playerInput.SpecialButton())
						{
							arcade = true;
							gs.audioManager.playSound("systemSelect");
							gs.Step = Game.StepType.SelectPlayer;
						}
						break;
					case 1:
						gameManagerSprites["arcade"].SetColor(1.0f, 1.0f, 1.0f, 1.0f);
						gameManagerSprites["survival"].SetColor(1.0f, 1.0f, 1.0f, 0.75f);

						if (gs.playerInput.SpecialButton())
						{
							arcade = false;
							gs.audioManager.playSound("systemSelect");
							gs.Step = Game.StepType.SelectPlayer;
						}
						break;
					default:
						break;
				}
				break;
			case Game.StepType.SelectPlayer:
				if (gs.playerInput.SpecialButton())
				{
					PlayerSelect playerSelect = (PlayerSelect)gs.Root.Search("playerSelect");

					Player player = (Player)gs.Root.Search("Player");
					player.Initialize(playerSelect.SelectShip());

					gs.Root.Search("bulletManager").Children.Clear();
					gs.Root.Search("enemyManager").Children.Clear();
					gs.Root.Search("enemyBulletManager").Children.Clear();
					gs.GameCounter = 0;
					gs.NumShips = 3;
					gs.Score = 0;

					if (arcade)
					{
						gs.levelParser.LoadLevel(gs, "/Application/assets/levels/Level1.tmx");
						gs.audioManager.ChangeSong("BlueSpace.mp3");
					}
					else
					{
						EnemyCommander enemyCommander =(EnemyCommander)gs.Root.Search("enemyCommander");
						enemyCommander.Status = Actor.ActorStatus.Action;
						enemyCommander.Initialize();
						gs.audioManager.ChangeSong("LongSeamlessLoop.mp3");
					}

					gs.Root.Search("playerSelect").Children.Clear();

					gs.Step = Game.StepType.Gameplay;
				}
				break;
			case Game.StepType.Gameplay:
#if DEBUG
				gs.debugString.WriteLine("Gameplay");
#endif
				// Game over if player stock falls to zero.
				if(gs.NumShips <= 0)
				{
					gs.Step= Game.StepType.GameOver;
					gs.audioManager.PauseSong();
					cnt=0;
				}
				
				++gs.GameCounter;

				if (gs.appCounter % 100 == 0)
					System.GC.Collect();

				break;
			case Game.StepType.GameOver:
#if DEBUG
				gs.debugString.WriteLine("GameOver");
#endif
				GameUI gameUI = (GameUI)gs.Root.Search("gameUI");

				if(gs.playerInput.StartButton())
				{
					PlayerSelect playerSelect = (PlayerSelect)gs.Root.Search("playerSelect");

					Player player =(Player)gs.Root.Search("Player");
					player.Status = Actor.ActorStatus.Action;
					player.Initialize(playerSelect.SelectShip());
					player.playerStatus = Player.PlayerStatus.Explosion;

					gameUI.ResetCountdown();
					gs.NumShips = 3;
					gs.audioManager.ResumeSong();
					gs.Step = Game.StepType.Gameplay;
				}
				else if(++cnt > 590)
				{
					gs.Root.Search("Player").Children.Clear();
					gs.Root.Search("enemyManager").Children.Clear();
					gs.Root.Search("enemyBulletManager").Children.Clear();
					gs.audioManager.ChangeSong("Music1.mp3");

					if (!arcade)
					{
						EnemyCommander enemyCommander =(EnemyCommander)gs.Root.Search("enemyCommander");
						enemyCommander.Status = Actor.ActorStatus.Rest;
						enemyCommander.resetWave();
					}
					
					gameUI.ResetCountdown();
					gs.Step= Game.StepType.Opening;
					System.GC.Collect();
				}
				
				break;
			default:
				throw new Exception("default in StepType.");
			}
			
			base.Update();
		}
	}
}