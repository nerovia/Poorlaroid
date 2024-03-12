using AForge.Video.DirectShow;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Poorlaroid
{
	internal class ViewModel : INotifyPropertyChanged
	{
		IPoorShader? _selectedShader;

		public event PropertyChangedEventHandler? PropertyChanged;

		public FilterInfoCollection FetchAvailableCameras() => new FilterInfoCollection(FilterCategory.VideoInputDevice);
		
	    public FilterInfo? SelectedCamera { get; set; }

		public List<IPoorShader> Shaders { get; } = new();

		public IPoorShader? SelectedShader 
		{
			get => _selectedShader;
			set => Change(ref _selectedShader, value);
		}

		public int SelectedShaderIndex
		{
			get => (SelectedShader is null) ? -1 : Shaders.IndexOf(SelectedShader);
			set => SelectedShader = (value == -1) ? null : Shaders[value];
		}

		public void CaptureImage()
		{

		}
		
		void Change<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
		{
			if (!Equals(field, newValue))
			{
				field = newValue;
				PropertyChanged?.Invoke(this, new(propertyName));
			}
		}
		
	}
}
