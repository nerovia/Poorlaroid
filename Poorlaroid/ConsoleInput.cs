using SadConsole.Input;
using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = SadConsole.Console;

namespace Poorlaroid
{
	internal class ConsoleInput : SadConsole.Components.IComponent
	{
		public uint SortOrder => 0;

		public bool IsUpdate => false;

		public bool IsRender => false;

		public bool IsMouse => false;

		public bool IsKeyboard => false;

		public void OnAdded(IScreenObject host)
		{
			if (_editor != null)
				throw new Exception();
			if (host is not Console console)
				throw new ArgumentException();
			_editor = console;
			_editor.Cursor.KeyboardPreview += KeyReceived;
		}

		public void OnRemoved(IScreenObject host)
		{
			_editor.Cursor.KeyboardPreview -= KeyReceived;
			_editor = null;
		}

		public void ProcessKeyboard(IScreenObject host, Keyboard keyboard, out bool handled) { throw new NotImplementedException(); }

		public void ProcessMouse(IScreenObject host, MouseScreenObjectState state, out bool handled) { throw new NotImplementedException(); }

		public void Render(IScreenObject host, TimeSpan delta) { throw new NotImplementedException(); }

		public void Update(IScreenObject host, TimeSpan delta) { throw new NotImplementedException(); }

		Console _editor;

		StringBuilder _sb = new StringBuilder();

		public event Action<string> InputReceived;

		protected virtual void OnInputReceived(string value) => InputReceived?.Invoke(value);

		public void ClearInput()
		{
			_editor.Clear(0, _editor.Cursor.Position.Y, _editor.Cursor.Position.X);
			_editor.Cursor.Position = _editor.Cursor.Position.WithX(0);
			_sb.Clear();
		}

		void KeyReceived(object sender, SadConsole.Input.KeyboardHandledKeyEventArgs e)
		{
			var cursor = (SadConsole.Components.Cursor)sender;
			switch (e.Key.Key)
			{
				case Keys.Up:
				case Keys.Down:
				case Keys.Left:
				case Keys.Right:
					e.IsHandled = true;
					break;

				case Keys.Back:
					if (_sb.Length > 0)
					{
						_sb.Remove(_sb.Length - 1, 1);
						_editor.Clear(cursor.Position.X, cursor.Position.Y);
					}
					else
					{
						e.IsHandled = true;
					}
					break;

				case Keys.Enter:
					_editor.Cursor.NewLine();
					OnInputReceived(_sb.ToString());
					_sb.Clear();
					e.IsHandled = true;
					break;

				default:
					if (!char.IsControl(e.Key.Character))
						_sb.Append(e.Key.Character);
					else
						e.IsHandled = true;
					break;

			}
		}
	}
}
