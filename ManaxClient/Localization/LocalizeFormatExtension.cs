using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace ManaxClient.Localization;

public class LocalizeFormatExtension(string key) : MarkupExtension
{
    public object? Param1 { get; set; }
    public object? Param2 { get; set; }
    public object? Param3 { get; set; }
    public object? Param4 { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        List<IBinding> bindings =
        [
            new Binding { Source = key }
        ];

        AddParameterBinding(bindings, Param1);
        AddParameterBinding(bindings, Param2);
        AddParameterBinding(bindings, Param3);
        AddParameterBinding(bindings, Param4);

        MultiBinding multiBinding = new()
        {
            Converter = new LocalizeFormatMultiConverter(),
            Bindings = bindings
        };

        return multiBinding;
    }

    private static void AddParameterBinding(List<IBinding> bindings, object? param)
    {
        switch (param)
        {
            case null:
                return;
            case IBinding binding:
                bindings.Add(binding);
                break;
            default:
                bindings.Add(new Binding { Source = param });
                break;
        }
    }
}