using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using DynamicData;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using OpcUAClient.Models;
using ReactiveUI.Fody.Helpers;

namespace OpcUAClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    [Reactive] public LineListenViewModel LineListenViewModel { get; set; }

    public MainWindowViewModel()
    {
        LineListenViewModel = new LineListenViewModel();
    }
}