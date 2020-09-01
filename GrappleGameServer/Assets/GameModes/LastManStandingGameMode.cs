﻿using SharedFiles.Utility;

namespace GameModes
{
	public class LastManStandingGameMode : GameMode
	{
		public static LastManStandingGameMode instance;
		
		public override void OnLoad()
		{
			instance = this;
			gameModeType = GameModeType.lastManStanding;
		}
		
		public override void OnUnload()
		{
			
		}

		public override void Update()
		{
			
		}
	}
}