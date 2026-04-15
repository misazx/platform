using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Audio;
using RoguelikeGame.UI.Panels;
using RoguelikeGame.Packages;
using System;

namespace RoguelikeGame.UI
{
	public partial class MainMenuScene : Control
	{
		[Signal]
		public delegate void StartGameRequestedEventHandler();

		[Signal]
		public delegate void SettingsRequestedEventHandler();

		[Signal]
		public delegate void QuitRequestedEventHandler();

		public override void _Ready()
		{
			GD.Print("[MainMenuScene] Ready - GDScript handles UI");
		}

		public override void _Input(InputEvent @event)
		{
		}
	}
}
