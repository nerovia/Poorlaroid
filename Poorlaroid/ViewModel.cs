using AForge.Video;
using AForge.Video.DirectShow;
using SadConsole;
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
		FilterInfo? _selectedCamera;
		VideoCaptureDevice? _cameraDevice;

		public event PropertyChangedEventHandler? PropertyChanged;

		public FilterInfoCollection FetchAvailableCameras() => new FilterInfoCollection(FilterCategory.VideoInputDevice);
		
	    public FilterInfo? SelectedCamera 
		{ 
			get => _selectedCamera; 
			set => Change(ref _selectedCamera, value, CameraChanged); }

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

		void CameraChanged(FilterInfo? camera)
		{
			if (_cameraDevice is not null)
			{
				_cameraDevice.SignalToStop();
				_cameraDevice.NewFrame -= NewFrame;
				_cameraDevice = null;
			}

			if (camera is not null)
			{
				_cameraDevice = new VideoCaptureDevice(camera.MonikerString);
				_cameraDevice.NewFrame += NewFrame;
				_cameraDevice.Start();
			}
		}

		private void NewFrame(object sender, NewFrameEventArgs eventArgs)
		{
			
		}

		void Change<T>(ref T field, T newValue, Action<T>? action = null, [CallerMemberName] string? propertyName = null)
		{
			if (!Equals(field, newValue))
			{
				field = newValue;
				action?.Invoke(field);
				PropertyChanged?.Invoke(this, new(propertyName));
			}
		}
		
	}
}
