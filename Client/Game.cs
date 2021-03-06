﻿using System.Numerics;
using System.Threading;
using Client.Authentication;
using Client.Authentication.Network;
using Client.UI;
using Client.World.Network;

namespace Client
{
	public interface IGame
	{
		BigInteger Key { get; }
		string Username { get; }

		IGameUI UI { get; }

		void ConnectTo(WorldServerInfo server);

		void Start();

		void Exit();
	}

	public class Game<T> : IGame
		where T : IGameUI, new()
	{
		bool Running;

		GameSocket socket;

		public BigInteger Key { get; private set; }
		public string Username { get; private set; }

		public IGameUI UI { get; protected set; }

		public Game(string username, string password)
		{
			Username = username;

			UI = new T();
			UI.Game = this;

			socket = new AuthSocket(this, username, password);
			socket.InitHandlers();
		}

		public void ConnectTo(WorldServerInfo server)
		{
			if (socket is AuthSocket)
				Key = ((AuthSocket)socket).Key;

			socket.Dispose();

			socket = new WorldSocket(this, server);
			socket.InitHandlers();

			if (socket.Connect())
				socket.Start();
			else
				Exit();
		}

		public void Start()
		{
			// the initial socket is an AuthSocket - it will initiate its own asynch read
			Running = socket.Connect();

			while (Running)
			{
				// main loop here
				Thread.Sleep(100);
			}

			UI.Exit();
		}

		public void Exit()
		{
			Running = false;
		}
	}
}