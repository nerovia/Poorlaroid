using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poorlaroid
{
	class SolidShader : SadShader
	{
		public override string Name { get; } = "Retro";

		public override void Render(Color input, ColoredGlyph output)
		{
			output.Background = input;
		}
	}

	class GlyphShader : SadShader
	{
		public override string Name => "Typewriter";

		readonly char[] shades = @" .'`^"",:;Il!i><~+_-?][}{1)(|\/tfjrxnuvczXYUJCLQ0OZmwqpdbkhao*#MW&8%B@$".ToCharArray();

		public override void Render(Color input, ColoredGlyph output)
		{
			var l = input.GetLuma() / 255f;
			output.Foreground = input.GetBrighter();
			output.Glyph = shades[(int)((shades.Length - 1) * l)];
		}
	}

	class RainbowShader : SadShader
	{
		public override string Name { get; } = "Microwaavee";
		byte maxL = 255;
		int[] histogram = new int[64];

		public Color[] palette = new Color[]
		{
			Color.Red,
			Color.Orange,
			Color.Yellow,
			Color.LimeGreen,
			Color.Turquoise,
			Color.DeepSkyBlue,
			Color.Blue,
			Color.Violet,
			Color.Magenta,
		};

		int[] shades = new[] { 0xB3, 0xBA, 0xBA };

		public override void Render(Color input, ColoredGlyph output)
		{
			var luma = input.GetLuma();
			//0x138, 0x13F,
			var s = (int)Helpers.Clerp(0, shades.Length - 1, luma / maxL);
			var c = (int)Helpers.Clerp(0, palette.Length - 1, luma / maxL);
			var t = (float)Game.Instance.GameRunningTotalTime.TotalSeconds / 2f;
			var h = (luma / 255f * 2f + t) % 1f;

			output.Foreground = Color.FromHSL(h, luma / 255f, luma / 255f);
			output.Glyph = 0x04;

			histogram[Math.Clamp((int)luma / 4, 0, 64)]++;
		}

		public override void End()
		{
			int index = 0;
			int luma = 0;
			for (var i = 0; i < histogram.Length; i++)
			{
				if (luma < (luma = histogram[i]))
					index = i;
				histogram[i] = 0;
			}

			maxL = (byte)((index * 4 + maxL) / 2);
		}
	}

	class CamsoleShader : SadShader
	{
		public override string Name => "Camsole";

		byte maxL = 255;
		int[] histogram = new int[64];

		static int Steps(byte input, byte steps, byte max)
		{
			for (int i = 0; i < steps; i++)
				if (input <= max * (i + 1) / steps)
					return 255 * i / steps;
			return 255;
		}

		public override void Render(Color input, ColoredGlyph output)
		{
			var r = Steps(input.R, 3, maxL);
			var g = Steps(input.G, 3, maxL);
			var b = Steps(input.B, 3, maxL);
			var color = new Color(r, g, b);
			var luma = input.GetLuma();

			var shade = (int)Helpers.Clerp(0x130, 0x135, luma / maxL);
			output.Foreground = color;
			output.Glyph = shade;

			histogram[Math.Clamp((int)luma / 4, 0, 64)]++;
		}

		public override void End()
		{
			int index = 0;
			int luma = 0;
			for (var i = 0; i < histogram.Length; i++)
			{
				if (luma < (luma = histogram[i]))
					index = i;
				histogram[i] = 0;
			}

			maxL = (byte)((index * 4 + maxL) / 2);
		}
	}

	class NoireShader : SadShader
	{
		public override string Name { get; } = "Noire";
		byte maxL = 255;
		int[] histogram = new int[64];
		int[] shades = new int[] { 0xB0, 0xB1, 0xB2, 0xDB };
		Color[] colors = new[] { Color.Black, Color.Gray, Color.White };
			//new Color[] { Color.Black, Color.DarkBlue, Color.DarkViolet, Color.Brown, Color.Pink, Color.White };
		
		

		public override void Render(Color input, ColoredGlyph output)
		{
			var luma = input.GetLuma();
			var i = (int)Helpers.Clerp(0, colors.Length * shades.Length, luma / maxL);
			var s = i % shades.Length;
			var f = Math.Min(i / shades.Length, colors.Length - 1);
			var b = Math.Max(f - 1, 0);

			output.Foreground = colors[f];
			output.Background = colors[b];
			output.Glyph = shades[s];

			//var s = (int)Helpers.Clerp(0, shades.Length - 1, luma / maxL);
			//output.Foreground = Color.White;
			//output.Glyph = shades[s];

			histogram[Math.Clamp((int)luma / 4, 0, 64)]++;
		}

		public override void End()
		{
			int index = 0;
			int luma = 0;
			for (var i = 0; i < histogram.Length; i++)
			{
				if (luma < (luma = histogram[i]))
					index = i;
				histogram[i] = 0;
			}

			maxL = (byte)((index * 4 + maxL) / 2);
		}
	}

	class OnionShader : SadShader
	{
		public override string Name { get; } = "Onio";

		public override void Render(Color input, ColoredGlyph output)
		{
			
		}
	}

	class PopShader : SadShader
	{
		public override string Name { get; } = "Pop";

		readonly Color[] colors = new[] { Color.Cyan, Color.Magenta, Color.Yellow, Color.Black };

		readonly int[] shades = new[] { 0x20, 0xB0, 0xB1, 0xB2 };

		float[] RGB2CMYK(Color color)
		{
			var rgb = new float[3];
			var cmyk = new float[4];

			var r = rgb[0] = color.R / 255f;
			var g = rgb[1] = color.G / 255f;
			var b = rgb[2] = color.B / 255f;

			var k = cmyk[3] = 1f - rgb.Max();

			cmyk[0] = (1 - r - k) / (1 - k);
			cmyk[1] = (1 - g - k) / (1 - k);
			cmyk[2] = (1 - b - k) / (1 - k);

			return cmyk;
		}

		public override void Render(Color input, ColoredGlyph output)
		{
			var cmyk = RGB2CMYK(input)
				.Select((q, i) => (q, i))
				.OrderBy(x => x.q)
				.ToArray();

			var f = cmyk[3];
			var b = cmyk[2];
			int s = f.q <= 0 ? 0 : (int)Helpers.Clerp(0, 2, b.q / f.q);

			var h = (int)input.GetHue() * 255 / 360;

			output.Background = Color.FromHSL(h / 255f, 0.8f, 0.5f); //colors[f.i];
			// output.Background = colors[b.i];
			// output.Glyph = shades[s];

		}
	}

}
