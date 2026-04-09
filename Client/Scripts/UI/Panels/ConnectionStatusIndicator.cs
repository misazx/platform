using Godot;
using RoguelikeGame.Network;

namespace RoguelikeGame.UI.Panels
{
	public class ConnectionStatusIndicator : Control
	{
		private ColorRect _indicatorLight;
		private Label _statusText;
		private AnimationPlayer _animationPlayer;

		public override void _Ready()
		{
			CustomMinimumSize = new Vector2(120, 30);

			var hbox = new HBoxContainer { SeparationOffset = 8 };
			AddChild(hbox);

			_indicatorLight = new ColorRect
			{
				CustomMinimumSize = new Vector2(16, 16),
				Size = new Vector2(16, 16)
			};
			hbox.AddChild(_indicatorLight);

			_statusText = new Label
			{
				Text = "离线",
				CustomMinimumSize = new Vector2(80, 20)
			};
			_statusText.AddThemeFontSizeOverride("font_size", 11);
			hbox.AddChild(_statusText);

			_animationPlayer = new AnimationPlayer();
			AddChild(_animationPlayer);

			CreateBlinkAnimation();

			UpdateStatus(NetworkState.Disconnected);
		}

		private void CreateBlinkAnimation()
		{
			var animation = new Animation();
			animation.Length = 1.0;
			animation.LoopMode = Animation.LoopModeEnum.Linear;

			int trackIndex = animation.AddTrack(Animation.TrackType.Value);
			animation.TrackSetPath(trackIndex, ":modulate:a");
			animation.TrackInsertKey(trackIndex, 0.0, 1.0);
			animation.TrackInsertKey(trackIndex, 0.5, 0.3);
			animation.TrackInsertKey(trackIndex, 1.0, 1.0);

			_animationPlayer.AddLibrary(animation);
			_animationPlayer.AssignAnimation("blink", animation);
		}

		public void UpdateStatus(NetworkState state)
		{
			Color lightColor;
			string text;

			switch (state)
			{
				case NetworkState.Disconnected:
					lightColor = Colors.Gray;
					text = "离线";
					StopBlink();
					break;

				case NetworkState.Connecting:
					lightColor = Colors.Yellow;
					text = "连接中";
					StartBlink();
					break;

				case NetworkState.Connected:
					lightColor = new Color(0.3f, 0.8f, 1f);
					text = "已连接";
					StopBlink();
					break;

				case NetworkState.Authenticating:
					lightColor = new Color(1f, 0.8f, 0.2f);
					text = "认证中";
					StartBlink();
					break;

				case NetworkState.Authenticated:
				case NetworkState.InLobby:
					lightColor = new Color(0.2f, 0.9f, 0.4f);
					text = "在线";
					StopBlink();
					break;

				case NetworkState.InRoom:
					lightColor = new Color(0.4f, 0.7f, 1f);
					text = "在房间";
					StopBlink();
					break;

				case NetworkState.InGame:
					lightColor = new Color(1f, 0.5f, 0.5f);
					text = "游戏中";
					StartBlink();
					break;

				default:
					lightColor = Colors.White;
					text = state.ToString();
					StopBlink();
					break;
			}

			_indicatorLight.Color = lightColor;
			_statusText.Text = text;
		}

		private void StartBlink()
		{
			if (_animationPlayer.IsPlaying()) return;
			_animationPlayer.Play("blink");
		}

		private void StopBlink()
		{
			if (_animationPlayer.IsPlaying())
			{
				_animationPlayer.Stop();
				_indicatorLight.Modulate = new Color(1, 1, 1, 1);
			}
		}
	}
}
