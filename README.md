# Squircle.Avalonia

<div align="center">
  <img src="https://github.com/user-attachments/assets/27ac857f-afbf-449b-a2af-7a6588fb5380">

  <h3>Figma's squircle implementation brought to Avalonia!</h3>
</div>

## Features

- Squircles!
- Corner Smoothing. You can choose smoothing you like, from `0` (no smoothing, basic rounded corners) to `1` (super smooth). Default value is `0.6` which is closest to IOS icons shape.
- Preserve Smoothing. When setting huge corner radius and there is not enough space for rounding and smoothing, this feature might come in handy if you want more pronounced results.
  
  > This feature is experimental and may give unexpected results when using with high Corner Smoothing.
- Styling. You can apply any styling you want. Corner Radius, Background, Borders (although Thickness can only be uniform), etc.

## Quick Start

To use this package in your Avalonia app, install it from [NuGet](https://www.nuget.org/packages/AlPi.Squircle.Avalonia/). For example, from your project's folder you can execute this command in terminal:
``` bash
dotnet add package AlPi.Squircle.Avalonia
```
After package is installed, you need to reference it in application's App.axaml file:
``` xaml
<Application
    ...
    xmlns:squircle="https://github.com/A1isandr/Squircle.Avalonia">
    
    <Application.Styles>
        ...
        <squircle:SquircleAvalonia/>
    </Application.Styles>
</Application>
```
Now you can draw Squircles by providing `xmlns:squircle="https://github.com/A1isandr/Squircle.Avalonia"` namespace as shown above and choosing `Squircle` control:
``` xaml
<squircle:Squircle
    Width="200"
    Height="200"
    CornerRadius="40"
    Background="CornflowerBlue"/>
```
After that you should see nice blue squircle on screen.

# Demo App

<div align="center">
  <img src="https://github.com/user-attachments/assets/e78f581a-c8ca-486c-b69b-252b3bfed709">
</div>

There is simple demo app where main features can be tested. It needs to be compiled from sources, no precompiled binaries provided at this moment.

## Credits

- Figma's [article](https://figma.com/blog/desperately-seeking-squircles/) about squircles.
- [phamfoo](https://github.com/phamfoo)'s [figma-squircle](https://github.com/phamfoo/figma-squircle/) repository and Preserve Smoothing feature.
- [MartinRGB](https://github.com/MartinRGB)'s [Figma_Squircle_Approximation](https://github.com/MartinRGB/Figma_Squircles_Approximation) repository.
