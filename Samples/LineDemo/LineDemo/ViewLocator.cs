using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaDemo.ViewModels;

namespace AvaloniaDemo;

public class ViewLocator : IDataTemplate
{
    public IControl Build(object data)
    {
        var viewModelName = data.GetType().FullName;
        if (string.IsNullOrWhiteSpace(viewModelName))
        {
            return new TextBlock { Text = "Not Found: " };
        }
        
        var viewTypeName = viewModelName.TrimEnd("Model".ToCharArray()).Replace("ViewModels","Views");
        var type = Type.GetType(viewTypeName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        
        return new TextBlock { Text = "Not Found: " + viewTypeName };
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}