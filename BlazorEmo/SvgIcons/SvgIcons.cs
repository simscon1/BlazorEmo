using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorEmo.SvgIcons;

/// <summary>
/// Provides SVG icon utilities for the BlazorEmo component.
/// </summary>
public static class SvgIcons
{
    /// <summary>
    /// Generates the BlazorEmo badge SVG with the specified size.
    /// </summary>
    /// <param name="size">The width and height of the SVG in pixels (default: 80).</param>
    /// <returns>An SVG string representing the BlazorEmo badge.</returns>
    public static string GetBadgeSvg(int size = 80)
    {
        return $$$"""
             
            """;
    }

    /// <summary>
    /// Generates the BlazorEmo badge SVG as a data URI for use in img src attributes.
    /// </summary>
    /// <param name="size">The width and height of the SVG in pixels (default: 80).</param>
    /// <returns>A data URI string containing the SVG.</returns>
    public static string GetBadgeSvgDataUri(int size = 80)
    {
        var svg = GetBadgeSvg(size);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }
 
    /// <summary>
    /// Generates the BlazorEmo logo SVG with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the SVG in pixels (default: 200).</param>
    /// <param name="height">The height of the SVG in pixels (default: 90).</param>
    /// <returns>An SVG string representing the BlazorEmo logo.</returns>
    public static string GetLogoSvg(int width = 200, int height = 90)
    {
        return $$$"""
             
            """;
    }

    /// <summary>
    /// Generates the BlazorEmo logo SVG as a data URI for use in img src attributes.
    /// </summary>
    /// <param name="width">The width of the SVG in pixels (default: 200).</param>
    /// <param name="height">The height of the SVG in pixels (default: 90).</param>
    /// <returns>A data URI string containing the SVG.</returns>
    public static string GetLogoSvgDataUri(int width = 200, int height = 90)
    {
        var svg = GetLogoSvg(width, height);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
        return $"data:image/svg+xml;base64,{base64}";
    }
     
}
