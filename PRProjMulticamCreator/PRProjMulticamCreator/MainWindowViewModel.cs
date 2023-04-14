using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using PRProjMulticamCreator.Core;

namespace PRProjMulticamCreator;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private bool _optionSelected;
    public MainWindowViewModel()
    {
        IsThreeCameraMode = false; // false by default
    }

    public bool IsThreeCameraMode
    {
        get => _optionSelected;
        set
        {
            _optionSelected = value;
            OnPropertyChanged();
        }
    }

    public DiluteMode DiluteModeValue { get; set; } = DiluteMode.TwoCameras;

    public List<DiluteMode> DiluteModeValues => Enum.GetValues(typeof(DiluteMode)).Cast<DiluteMode>().ToList();

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
