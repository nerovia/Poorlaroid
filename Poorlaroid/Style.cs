using SadConsole;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poorlaroid
{
	static class Style
	{
		public readonly static Color AppBackground = Color.AntiqueWhite;
		public readonly static Color AppForeground = Color.White;

		public readonly static Color ConsoleBackground = Color.Transparent;
		public readonly static Color ConsoleForeground = Color.Transparent;
		public readonly static Color ConsoleCursorBackground = Color.Black;
		public readonly static Color ConsoleCursorForeground = Color.White;

		public readonly static Color[] RainbowColors =
		[
			Color.Magenta,
			Color.Red,
			Color.Orange,
			Color.Yellow,
			Color.LimeGreen,
			Color.DeepSkyBlue
		];

		public readonly static Colors AccentColors = new()
		{
			Appearance_ControlNormal = new ColoredGlyph(Color.White, Color.Red),
			Appearance_ControlFocused = new ColoredGlyph(Color.White, Color.Red)
		};

		public readonly static Colors ControlColors = new()
		{
			Appearance_ControlNormal = new ColoredGlyph(Color.Black, Color.SandyBrown),
			Appearance_ControlFocused = new ColoredGlyph(Color.Black, Color.SandyBrown),
			Appearance_ControlSelected = new ColoredGlyph(Color.Red, Color.Transparent)
		};
	}
}
