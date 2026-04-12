using System;
using System.Collections.Generic;
using Godot;

namespace RoguelikeGame.Network.Core
{
	public partial class NetworkClock : Node
	{
		private static NetworkClock _instance;

		public static NetworkClock Instance => _instance;

		private List<long> _rttSamples = new();
		private long _averageRTT = 0;
		private long _serverTimeOffset = 0;
		private int _maxSampleSize = 20;
		private Timer _syncTimer;

		public long AverageRTT => _averageRTT;
		public long ServerTimeOffset => _serverTimeOffset;
		public long EstimatedServerTime => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _serverTimeOffset;

		public override void _Ready()
		{
			if (_instance != null && _instance != this)
			{
				QueueFree();
				return;
			}

			_instance = this;

			_syncTimer = new Timer
			{
				WaitTime = 2.0,
				OneShot = false,
				Autostart = true
			};
			_syncTimer.Connect("timeout", new Callable(this, nameof(PerformSync)));
			AddChild(_syncTimer);

			GD.Print("[NetworkClock] 时钟同步服务已启动");
		}

		public void PerformSync()
		{
			if (!NetworkManager.Instance?.IsOnline ?? true)
				return;

			var clientTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			var pingPacket = PacketSerializer.SerializePacket(
				PacketType.Ping,
				new { ClientTimestamp = clientTime }
			);

			ConnectionManager.Instance?.SendAsync(pingPacket);
		}

		public void RecordPong(long clientSendTime, long serverReceiveTime, long serverSendTime)
		{
			long clientReceiveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			long rtt = clientReceiveTime - clientSendTime;

			if (rtt <= 0) return;

			_rttSamples.Add(rtt);

			if (_rttSamples.Count > _maxSampleSize)
			{
				_rttSamples.RemoveAt(0);
			}

			long sum = 0;
			foreach (var sample in _rttSamples)
			{
				sum += sample;
			}
			_averageRTT = sum / _rttSamples.Count;

			long oneWayLatency = _averageRTT / 2;
			_serverTimeOffset = serverSendTime + oneWayLatency - clientReceiveTime;

			GD.Print($"[NetworkClock] RTT: {_averageRTT}ms, Offset: {_serverTimeOffset}ms");
		}

		public void Reset()
		{
			_rttSamples.Clear();
			_averageRTT = 0;
			_serverTimeOffset = 0;

			GD.Print("[NetworkClock] 已重置时钟同步数据");
		}

		public override void _ExitTree()
		{
			_instance = null;
		}
	}
}
