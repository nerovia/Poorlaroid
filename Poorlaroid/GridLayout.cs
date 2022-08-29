using SadConsole;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nerovia.Toolkit
{
	public static class Extensions
	{
		static int Center(int total, int width) => (total - width) / 2;
		static int Right(int total, int width) => total - width;

		public static void PlaceWithin(this ControlBase control, Rectangle area, Direction.Types direction = Direction.Types.UpLeft)
		{
			Point pos;
			switch (direction)
			{
				case Direction.Types.None:
					pos = new(area.X + Center(area.Width, control.Width), area.Y + Center(area.Height, control.Height));
					break;
				case Direction.Types.Up:
					pos = new(area.X + Center(area.Width, control.Width), area.Y);
					break;
				case Direction.Types.UpRight:
					pos = new(area.X + Right(area.Width, control.Width), area.Y);
					break;
				case Direction.Types.Right:
					pos = new(area.X + Right(area.Width, control.Width), area.Y + Center(area.Height, control.Height));
					break;
				case Direction.Types.DownRight:
					pos = new(area.X + Right(area.Width, control.Width), area.Y + Right(area.Height, control.Height));
					break;
				case Direction.Types.Down:
					pos = new(area.X + Center(area.Width, control.Width), area.Y + Right(area.Height, control.Height));
					break;
				case Direction.Types.DownLeft:
					pos = new(area.X, area.Y + Right(area.Height, control.Height));
					break;
				case Direction.Types.Left:
					pos = new(area.X, area.Y + Center(area.Height, control.Height));
					break;
				case Direction.Types.UpLeft:
					pos = new(area.X, area.Y);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			control.Position = pos;
		}
	}

	public class GridLayout : IEnumerable<Rectangle>
	{
		public int Width { get => Area.Width; }

		public int Height { get => Area.Height; }

		public Point Spacing { get; }

		public Point Position { get => Area.Position; }

		public Rectangle Area { get; }

		public GridLayout(Rectangle area, GridLength[] columns, GridLength[] rows, Point spacing = default)
		{
			Area = area;
			Spacing = spacing;
			Columns = DetermineLengths(Width, spacing.X, columns);
			Rows = DetermineLengths(Height, spacing.Y, rows);
		}

		public GridLayout(int width, int height, GridLength[] columns, GridLength[] rows, Point spacing = default)
			: this(new(0, 0, width, height), columns, rows, spacing)
		{
			
		}

		static (int, int)[] DetermineLengths(int total, int spacing, GridLength[] input)
		{
			var output = new (int i, int length)[Math.Max(input.Length, 1)];

			if (input.Length <= 0)
			{
				output[0].i = spacing;
				output[0].length = total - spacing;
			}
			else
			{
				var remainder = total - (input.Length + 1) * spacing;
				var proportionSum = 0;

				for (int i = 0; i < output.Length; i++)
				{
					switch (input[i].Unit)
					{
						case GridUnitType.Absolute:
							remainder -= input[i].Value = Math.Min(input[i].Value, remainder);
							break;
						case GridUnitType.Relative:
							proportionSum += input[i].Value;
							break;
					}
				}

				remainder = Math.Max(remainder, 0);
				proportionSum = Math.Max(proportionSum, 1);

				for (int i = 0, previous = 0; i < output.Length; i++)
				{
					output[i].i = previous + spacing;
					switch (input[i].Unit)
					{
						case GridUnitType.Absolute:
							output[i].length = input[i].Value;
							break;
						case GridUnitType.Relative:
							output[i].length = input[i].Value * remainder / proportionSum;
							break;
					}
					previous = output[i].i + output[i].length;
				}
			}



			return output;
		}

		readonly (int y, int height)[] Rows;
		readonly (int x, int width)[] Columns;

		public Rectangle this[int column, int row]
		{
			get => new Rectangle(Columns[column].x + Position.X, Rows[row].y + Position.Y, Columns[column].width, Rows[row].height);
		}

		public delegate T LayoutHandler<T>(Rectangle cell) where T : ControlBase;

		public void Place<T>(int x, int y, T control, Padding padding = default) where T : ControlBase
		{
			control.Position = this[x, y].Position + new Point(padding.Top, padding.Right);
		}

		public T Place<T>(int x, int y, LayoutHandler<T> handler, Padding padding = default) where T : ControlBase
		{
			var cell = this[x, y].Translate(padding.DeltaPosition).ChangeSize(padding.DeltaSize);
			var control = handler.Invoke(cell);
			control.Position = cell.Position;
			return control;
		}

		public T Place<T>(int x, int y, ushort columnSpan, ushort rowSpan, LayoutHandler<T> handler, Padding padding = default) where T : ControlBase
		{
			var from = this[x, y];
			var to = this[x + columnSpan, y + rowSpan];
			var cell = new Rectangle(from.X, from.Y, to.X - from.X + to.Width, to.Y - from.Y + to.Height);
			var control = handler.Invoke(cell);
			control.Position = from.Position;
			return control;
		}

		public IEnumerator<Rectangle> GetEnumerator()
		{
			for (int y = 0; y < Rows.Length; y++)
				for (int x = 0; x < Columns.Length; x++)
					yield return this[x, y];
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public enum GridUnitType
	{
		Absolute,
		Relative,
	}

	public struct GridLength
	{
		public GridLength(int value, GridUnitType unit)
		{
			Value = value;
			Unit = unit;
		}

		public GridLength(int value) : this(value, GridUnitType.Absolute)
		{

		}

		public int Value { get; set; }
		public GridUnitType Unit { get; set; }
		public bool IsAbsolute { get => Unit == GridUnitType.Absolute; }
		public bool IsRelative { get => Unit == GridUnitType.Relative; }

		public static implicit operator GridLength(int i)
		{
			return new(i);
		}

		public static implicit operator GridLength(string s)
		{
			if (string.IsNullOrEmpty(s))
				return new(0);

			var match = Regex.Match(s, @"^(\*?)(\d*)$");

			if (!match.Success)
				throw new ArgumentException();

			var hasStar = match.Groups[1].Length > 0;

			var hasValue = match.Groups[2].Length > 0;

			GridUnitType unit = hasStar ? GridUnitType.Relative : GridUnitType.Absolute;

			int value = hasValue ? int.Parse(match.Groups[2].Value) : (hasStar ? 1 : 0);

			return new(value, unit);
		}

		public override string ToString()
		{
			switch (Unit)
			{
				case GridUnitType.Absolute:
					return $"{Value}";
				case GridUnitType.Relative:
					return $"*{Value}";
				default:
					return base.ToString();
			}
		}
	}

	public struct Padding
	{
		public Padding(int size) : this(size, size)
		{

		}

		public Padding(int horizontal, int vertical) : this(horizontal, vertical, horizontal, vertical)
		{

		}

		public Padding(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public Point DeltaPosition { get => new(Left, Top); }
		public Point DeltaSize { get => new Point(-Left - Right, -Top - Bottom); }

		public static implicit operator Padding(int size)
		{
			return new(size);
		}

		public static implicit operator Padding(SadRogue.Primitives.Point point)
		{
			return new(point.X, point.Y);
		}
	}
}
