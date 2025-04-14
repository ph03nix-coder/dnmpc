using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace dnmpc {

    public class MPClient {

        TcpClient client;

        public MPClient() {
            this.client = new TcpClient();
        }
        public bool Connect() {
            try {
                this.client.Connect(IPAddress.Parse("127.0.0.1"), 2806);
                return true;
            } catch (SocketException ex) {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public string Send(string data) {
            byte[] buffer = System.Text.Encoding.Default.GetBytes(data);
            try {
                this.client.Client.Send(buffer);
                return Receive();
            } catch {
                return "N/A";
            }
        }

        public string Receive() {
            byte[] buffer = new byte[1024];
            try {
                this.client.Client.Receive(buffer);
                return System.Text.Encoding.Default.GetString(buffer);
            } catch {
                return "";
            }
        }

		public string GetCurrentSong() {
			return Send("get_current_song");
		}

		public void Play() {
			Send("play");
		}

		public void Pause() {
			Send("pause");
		}

		public void Resume() {
			Send("resume");
		}

		public void Next() {
			Send("next");
		}

		public void Prev() {
			Send("prev");
		}

		public void VolumeUp() {
			Send("volume_up");
		}

		public void VolumeDown() {
			Send("volume_down");
		}

		public float GetVolume() {
			try {
				return float.Parse(Send("get_volume"));
			} catch {
				return 0.0f;
			}
		}

		public void Forward(bool long_forward = false) {
			if(long_forward) Send("long_forward");
			else Send("forward");
		}

		public void Backward(bool long_backward = false) {
			if(long_backward) Send("long_backward");
			else Send("backward");
		}

		public double GetPosition() {
			try {
				return double.Parse(Send("get_position"));
			} catch {
				return 0.0;
			}
		}

		public double GetLength() {
			try {
				return double.Parse(Send("get_length"));
			} catch {
				return 0.0;
			}
		}

		public int GetPlaylistIndex() {
			try {
				return int.Parse(Send("get_pl_index"));
			} catch {
				return 0;
			}
		}

		public int GetPlaylistLength() {
			try {
				return int.Parse(Send("get_pl_length"));
			} catch {
				return 0;
			}
		}

		public string GetPlayerState() {
			return Send("get_player_state").Replace("\0", "");
		}

		public bool IsPaused() {
			return GetPlayerState() == "paused";
		}

		public bool IsPlaying() {
			return GetPlayerState() == "playing";
		}
    }

    public static class Program {

        public static int startx, starty;

        public static void WriteAt(string data, int x, int y) {
			try {
				Console.SetCursorPosition(startx + x, starty + y);
			} catch {
				starty -= 2;
			}
			Console.WriteLine(data.PadRight(Console.BufferWidth));
			Console.Out.Flush();
        }

		public static void UpdateTerm(MPClient cl) {
			string current_song = cl.GetCurrentSong().Replace("\0", "");
			int pl_index		= cl.GetPlaylistIndex();
			int pl_length		= cl.GetPlaylistLength();

			double snd_pos		= cl.GetPosition();
			double snd_len		= cl.GetLength();
			float vol			= cl.GetVolume();

			TimeSpan ts_snd_pos	= TimeSpan.FromSeconds(snd_pos);
			TimeSpan ts_snd_len = TimeSpan.FromSeconds(snd_len);
			
			string line1 = $"Playing: ({pl_index}/{pl_length}) {current_song}";
			string line2 = $"{ts_snd_pos.ToString(@"hh\:mm\:ss")}/{ts_snd_len.ToString(@"hh\:mm\:ss")} -- Volume: {vol * 100}% {((cl.IsPaused()) ? "(Paused)" : "")}";
			
			WriteAt(line1, 0, 0);
			WriteAt(line2, 0, 1);
		}

        public static void Main(string[] args) {
            
            startx = Console.CursorLeft;
            starty = Console.CursorTop;

			Console.CursorVisible = false;
            
            MPClient client = new MPClient();
            client.Connect();

            Thread updater = new Thread(() => {
				while(true) {
					UpdateTerm(client);
					Thread.Sleep(1000);
				}
            });

			updater.Start();

            while(true) {
                var key = Console.ReadKey(true);

				switch(key.KeyChar) {
					case 'q':
						Console.CursorVisible = true;
						Environment.Exit(0);
						break;
					
					case '\n':
					case '>':
						client.Next();
						break;
					case '<':
						client.Prev();
						break;
					
					case ' ':
						if(client.IsPaused()) client.Resume();
						else if(client.IsPlaying()) client.Pause();
						break;
					
					default: {
						switch(key.Key) {
							case ConsoleKey.LeftArrow:
								client.Backward(key.Modifiers == ConsoleModifiers.Shift);
								break;
							case ConsoleKey.RightArrow:
								client.Forward(key.Modifiers == ConsoleModifiers.Shift);
								break;
							case ConsoleKey.UpArrow:
								client.VolumeUp();
								break;
							case ConsoleKey.DownArrow:
								client.VolumeDown();
								break;
						}
						break;
					}
				}

				lock (client) {
					UpdateTerm(client);
				}
            }
        }
    }
}