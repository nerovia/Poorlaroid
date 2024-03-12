using Nerovia.Toolkit;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.ComponentModel;
using Console = SadConsole.Console;

namespace Poorlaroid
{
	class View : ControlsConsole
	{
		public ViewModel ViewModel { get; }
		public bool IsInitialized { get; set; }

		public Console? CommandConsole;
		public ScreenSurface? CameraView;
		public Button? CaptureButton;
		public Button? CommandToggle;
		public Button? ShaderToggle;

		public View(ViewModel viewModel, int width, int height) : base(width, height)
		{
			ViewModel = viewModel;
			ViewModel.PropertyChanged += (s, e) => Refresh();
		}

		public void Init()
		{	
			var mainGrid = new GridLayout(Width, Height, ["*"], ["*", "3"], new(4, 2));
			var menuGrid = new GridLayout(mainGrid[0, 1].Expand(4, 0), ["*", "14", "*"], ["*"], new(4, 0));

			var viewRect = mainGrid[0, 0].Expand(-2, -1);
			var controlRect = mainGrid[0, 1];

			// Cleanup
			Children.Clear();
			Controls.Clear();

			// Draw Background
			Surface.DefaultBackground = Style.AppBackground;
			Surface.DefaultForeground = Style.AppForeground;
			Surface.Clear();
			Surface.Fill(mainGrid[0, 0], background: Color.Black);

			// Draw Rainbow
			var rainbowThickness = 3;
			var rainbowRect = new Rectangle(0, (viewRect.Height - Style.RainbowColors.Length * rainbowThickness) / 2 + viewRect.Y, Width, rainbowThickness);
			foreach (var color in Style.RainbowColors)
			{
				Surface.Fill(rainbowRect, background: color);
				rainbowRect = rainbowRect.TranslateY(rainbowRect.Height);
			}

			// Create Command Console
			CommandConsole = new Console(viewRect.Width, viewRect.Height)
			{
				Position = viewRect.Position,
				FocusOnMouseClick = true,
				SadComponents = { new ConsoleInput() }
			};
			CommandConsole.Surface.DefaultBackground = Style.ConsoleBackground;
			CommandConsole.Surface.DefaultForeground = Style.ConsoleForeground;
			CommandConsole.Cursor.PrintAppearanceMatchesHost = false;
			CommandConsole.Cursor.PrintAppearance.Background = Style.ConsoleCursorBackground;
			CommandConsole.Cursor.PrintAppearance.Foreground = Style.ConsoleCursorForeground;
			Children.Add(CommandConsole);

			// Create Camera View
			CameraView = new ScreenSurface(viewRect.Width, viewRect.Height) { Position = viewRect.Position };
			Children.Add(CameraView);

			// Create Shader Toggle Button
			ShaderToggle = menuGrid.Place(0, 0, cell => new Button(cell.Width, cell.Height) 
			{ 
				Text = "SHADERS",
				ShowEnds = false,
			});
			ShaderToggle.SetThemeColors(Style.ControlColors);
			Controls.Add(ShaderToggle);

			// Create Capture Button
			CaptureButton = menuGrid.Place(1, 0, cell => new Button(cell.Width, cell.Height) 
			{ 
				Text = "CAPTURE",
				ShowEnds = false,
			});
			CaptureButton.SetThemeColors(Style.AccentColors);
			Controls.Add(CaptureButton);

			// Create Command Toggle Button
			CommandToggle = menuGrid.Place(2, 0, cell => new Button(cell.Width, cell.Height)
			{
				Text = "COMMAND",
				ShowEnds = false,
			});
			CommandToggle.SetThemeColors(Style.ControlColors);
			Controls.Add(CommandToggle);

			IsInitialized = true;

			Refresh();
		}

		public void Refresh()
		{
			if (!IsInitialized)
				return;
			ShaderToggle!.Text = ViewModel.SelectedShader?.Name.ToUpper() ?? "NO SHADER";
			CaptureButton!.IsEnabled = ViewModel.SelectedCamera != null;
		}

		void ShuffleShader()
		{
			var index = ViewModel.SelectedShaderIndex;
			ViewModel.SelectedShaderIndex = ++index > ViewModel.Shaders.Count ? -1 : index;
		}

		void CaptureImage() => ViewModel.CaptureImage();
	}
}
