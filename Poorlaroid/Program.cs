using AForge.Video;
using AForge.Video.DirectShow;
using Nerovia.Toolkit;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Console = SadConsole.Console;
using Rectangle = System.Drawing.Rectangle;
using Color = SadRogue.Primitives.Color;
using System.Runtime.InteropServices;
using System.Threading;
using SadConsole.Input;
using System.Text;
using System.Speech.Recognition;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SadConsole.Configuration;

namespace Poorlaroid
{
	public static class Helpers
	{
		public static float Clerp(float a, float b, float p)
		{
			var v = p * (b - a) + a;
			return Math.Clamp(v, a, b);
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct RGB
	{
		public RGB(byte r, byte g, byte b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		[FieldOffset(2)] public byte r;
		[FieldOffset(1)] public byte g;
		[FieldOffset(0)] public byte b;
	}

	interface IPoorShader
	{
		public string Name { get; }

		public void Begin();

		public void End();

		public void Render(Color input, ColoredGlyph output);
	}

	internal abstract class SadShader : IPoorShader
	{
		public abstract string Name { get; }

		public override string ToString() => Name;

		public virtual void Begin() { }

		public virtual void End() { }

		public abstract void Render(Color input, ColoredGlyph output);
	}

	
	internal class Program
	{
		enum State
		{
			Init,
			Options,
			Camera,
			Capture,
			Suspending,
		}

		const int ScreenWidth = 76;
		const int ScreenHeight = 43;

		State _state;
		VideoCaptureDevice _camera;
		FilterInfo[] _cams;
		IPoorShader _shader;
		int _selectedShader = -1;
		bool _flip;
		CancellationTokenSource _renderCancel;
		static Lazy<string> _saveFolder = new Lazy<string>(() =>
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Poorlaroid");
			Directory.CreateDirectory(path);
			return path;
		});

		static IPoorShader[] _shaders =
		[
			
		];

	
		private static void Main(string[] args)
		{
			var viewModel = new ViewModel()
			{
				Shaders =
				{
					new SolidShader(),
					new GlyphShader(),
					new CamsoleShader(),
					new RainbowShader(),
					new NoireShader(),
					new OnionShader(),
				}
			};

			var cams = viewModel.FetchAvailableCameras();
			viewModel.SelectedCamera = cams.Count > 0 ? cams[0] : null;

			Settings.WindowTitle = "Poorlaroid";
			Settings.UseDefaultExtendedFont = true;

			var config = new SadConsole.Configuration.Builder()
				.SetScreenSize(ScreenWidth, ScreenHeight)
				.SetStartingScreen(game => new View(viewModel, game.ScreenCellsX, game.ScreenCellsY))
				.OnStart(Start)
				.OnEnd(End);

			Game.Create(config);
			Game.Instance.Run();
			Game.Instance.Dispose();
		}

		static void Start(object sender, GameHost host)
		{
			if (host.Screen is View view)
				view.Init();
		}

		static void End()
		{

		}

		private void ChangeState(State value)
		{
			if (_state != value)
			{
				ChangeStateCore(_state, value);
				_state = value;
			}
		}

		private void ChangeStateCore(State oldState, State newState)
		{
			switch (oldState)
			{
				case State.Camera:
					break;
				case State.Options:
					_console.Cursor.IsEnabled = false;
					_console.Cursor.IsVisible = false;
					ClearOptions();
					break;
			}

			switch (newState)
			{
				case State.Camera:
					//_capture.IsEnabled = true;
					//_shaderSwap.IsEnabled = true;
					_optionsButton.Text = "OPTIONS";
					break;

				case State.Options:
					//_capture.IsEnabled = false;
					//_shaderSwap.IsEnabled = false;
					_optionsButton.Text = "CLOSE";
					_console.IsFocused = true;
					_console.Cursor.IsEnabled = true;
					_console.Cursor.IsVisible = true;
					break;

				case State.Suspending:
					SetCamera(null);
					break;
			}
		}

		void OnEnd(object sender, GameHost host)
		{
			ChangeState(State.Suspending);
		}

		void OnStart(object sender, GameHost host)
		{
			BuildUI();
			SwapShader();

			var info = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			_cams = new FilterInfo[info.Count];
			for (int i = 0; i < info.Count; i++)
				_cams[i] = info[i];

			if (_cams.Count() <= 0)
			{
				ChangeState(State.Options);
				_console.Cursor.Print("there are no cameras available");
				_console.Cursor.Position = new(0, 1);
			}
			else
			{
				SetCamera(_cams[0]);
				ChangeState(State.Camera);
			}
		}

		async void AnimateCapture()
		{
			var delay = TimeSpan.FromSeconds(0.2);
			_view.Surface.SetEffect(_view.Surface, new SadConsole.Effects.Fade()
			{
				AutoReverse = true,
				FadeDuration = delay,
				FadeForeground = true,
				FadeBackground = true,
				UseCellBackground = true,
				UseCellForeground = true,
				DestinationBackground = new(Color.White, Color.White),
				DestinationForeground = new(Color.White, Color.White),
				RemoveOnFinished = true,
			});
			await Task.Delay(delay);
			ChangeState(State.Camera);
		}

		void ClearOptions()
		{
			_console.Cursor.Move(0, 0);
					_console.Clear();
		}

		void SetCamera(FilterInfo info)
		{
			if (_camera != null)
			{
				_camera.SignalToStop();
				_camera.NewFrame -= FrameRender;
			}

			if (info == null)
				_camera = null;
			else
				_camera = new VideoCaptureDevice(info.MonikerString);

			if (_camera != null)
			{
				_camera.NewFrame += FrameRender;
				_camera.Start();
				if (Regex.IsMatch(info.Name, "front", RegexOptions.IgnoreCase))
					_flip = true;
				else if (Regex.IsMatch(info.Name, "rear", RegexOptions.IgnoreCase))
					_flip = false;
			}
		}


		private bool IsMatch(string s, string pattern, out Match match)
		{
			match = Regex.Match(s, pattern);
			return match.Success;
		}

		private void InputReceived(string s)
		{
			if (_state != State.Options)
				return;

			switch (s)
			{
				case "cams":
					Print("AVAILABLE CAMERAS:");
					Print(_cams.Select((x, i) => $"[{i}] {x.Name}").ToArray());
					break;

				case "help":
					Print("RECOGNIZED COMANDS:");
					Print("help", "cams", "cam", "flip", "clear", "exit");
					break;

				case "flip":
					Print("FLIP WAS FLIPPED");
					_flip = !_flip;
					break;

				case "clear":
					ClearOptions();
					break;

				case "exit":
					ChangeState(State.Camera);
					break;

				default:
					Match match;
					if (IsMatch(s, @"^cam (\d?)$", out match))
					{
						if (match.Groups[1].Length <= 0)
						{
							Print("PLEASE SPECIFY A NUMBER");
						}
						else
						{
							var i = int.Parse(match.Groups[1].Value);

							if (i >= 0 && i < _cams.Length)
							{
								var cam = _cams[i];
								SetCamera(cam);
								Print($"CAMERA SELECTED: {cam.Name}");
							}
							else
							{
								Print("CAMERA UNAVALIABLE");
							}
						}
					}
					else if (IsMatch(s, @"^shader (\w+)$", out match))
					{
						if (match.Groups[1].Length <= 0)
						{
							Print("PLEASE NAME A SHADER");
						}
						else
						{
							var name = match.Groups[1].Value.ToLower();
							var shader = _shaders.FirstOrDefault(x => x.Name.ToLower() == name);
							if (shader != null)
							{
								SwapShader(shader);
								Print("SHADER APPLIED");
							}
							else
							{
								Print("SHADER UNAVALIABLE");
							}
						}
					}
					else
					{
						Print("QUERE NOT RECOGNIZED");
					}
					break;
			}
		}

		private void Print(params string[] text)
		{
			foreach (var s in text)
				_console.Cursor.Print($"> {s}").NewLine();
		}

		private void OptionsClicked(object sender, EventArgs e)
		{
			switch (_state)
			{
				case State.Camera:
					ChangeState(State.Options);
					break;
				case State.Options:
					ChangeState(State.Camera);
					break;
			}
		}

		private void SwapShader()
		{
			if (++_selectedShader >= _shaders.Length)
				_selectedShader = 0;
			SwapShader(_shaders[_selectedShader]);
		}

		private void SwapShader(IPoorShader shader)
		{
			_shader = shader;
			_shaderButton.Text = _shader.Name.ToUpper();
			_renderCancel?.Cancel();
			_view.Surface.Clear();
		}


		private void ShaderSwapped(object sender, EventArgs e)
		{
			SwapShader();
		}

		private async void CaptureClick(object sender, EventArgs e)
		{
			ChangeState(State.Capture);
			var output = _view.Renderer.Output;
			using (var bitmap = await CreateBitmap(output.Width, output.Height, output.GetPixels()))
				Save(bitmap);
			AnimateCapture();
		}

		static void Save(Bitmap bitmap)
		{
			var folder = _saveFolder.Value; // Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			var path = Path.Combine(folder, $"poorlaroid{DateTime.Now:yyMMdd_HH_mm_ss)}.bmp");
			bitmap.Save(path);
		}

		private static unsafe Task<Bitmap> CreateBitmap(int width, int height, Color[] pixels)
		{
			var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			var data = bitmap.LockBits(new(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			RGB* ptr = (RGB*)data.Scan0;
			for (int i = 0; i < pixels.Length; i++)
				ptr[i] = new RGB(pixels[i].R, pixels[i].G, pixels[i].B);
			bitmap.UnlockBits(data);
			return Task.FromResult(bitmap);
		}

		private unsafe Task Render(Bitmap frame, CancellationToken cancel)
		{
			var shader = _shader;
			var canvas = _view.Surface;
			var data = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			RGB* ptr = (RGB*)data.Scan0.ToPointer();

			int h = Math.Min(frame.Height, frame.Width) / canvas.Height;
			int w = Math.Min(frame.Height, frame.Width) / canvas.Width;

			int q = (frame.Height - h * canvas.Height) / 2;
			int p = (frame.Width - w * canvas.Width) / 2;

			shader.Begin();

			for (int y = 0; y < canvas.Height; y++)
			{
				for (int x = 0; x < canvas.Width; x++)
				{
					if (cancel.IsCancellationRequested)
						return Task.FromCanceled(cancel);
					var rgb = ptr[(y * h + q) * frame.Width + (x * w + p)];
					var color = new Color(rgb.r, rgb.g, rgb.b);
					if (_flip)
						shader.Render(color, canvas[canvas.Width - 1 - x, y]);
					else
						shader.Render(color, canvas[x, y]);
				}
			}
			canvas.IsDirty = true;
			shader.End();

			return Task.CompletedTask;
		}

		private void Render()
		{
			
		}

		private async void FrameRender(object sender, NewFrameEventArgs eventArgs)
		{
			if (_state == State.Capture)
				return;

			try
			{
				_renderCancel = new();
				await Render(eventArgs.Frame, _renderCancel.Token);
			}
			catch (TaskCanceledException )
			{

			}
			finally
			{
				var source = _renderCancel;
				_renderCancel = null;
				source.Dispose();
			}
		}
	}
}