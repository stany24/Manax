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

    public static int NbParameters => 4;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        List<IBinding> bindings = [];
        
        AddParameterBinding(bindings, Param1 ?? string.Empty);
        AddParameterBinding(bindings, Param2 ?? string.Empty);
        AddParameterBinding(bindings, Param3 ?? string.Empty);
        AddParameterBinding(bindings, Param4 ?? string.Empty);

        MultiBinding multiBinding = new()
        {
            Converter = new LocalizeFormatMultiConverter(key),
            Bindings = bindings
        };

        return multiBinding;
    }

    private static void AddParameterBinding(List<IBinding> bindings, object? param)
    {
        if (param is IBinding binding)
        {
            bindings.Add(binding);
        }
        else
        {
            bindings.Add(new Binding { Source = param });
        }
    }
}