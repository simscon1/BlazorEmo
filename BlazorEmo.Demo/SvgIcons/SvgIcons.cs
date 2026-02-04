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
    /// Generates the BlazorEmo logo SVG with the specified dimensions.
    /// </summary>
    /// <param name="width">The width of the SVG in pixels (default: 200).</param>
    /// <param name="height">The height of the SVG in pixels (default: 90).</param>
    /// <returns>An SVG string representing the BlazorEmo logo.</returns>
    public static string GetLogoSvg(int width = 200, int height = 90)
    {
         return $$$"""
            <svg width={width} height={height} viewBox="0 0 200 90" fill="none" xmlns="http://www.w3.org/2000/svg">
              {/* Document/Card icon with emoji face */}
              <g>
                <path
                  d="M20 20 L20 60 L52 60 L52 32 L40 20 Z"
                  fill="white"
                  stroke="#7C3AED"
                  strokeWidth="2.5"
                />
                <path 
                  d="M40 20 L40 32 L52 32" 
                  fill="#E9D5FF" 
                  stroke="#7C3AED" 
                  strokeWidth="2" 
                  strokeLinejoin="round" 
                />

                {/* Emoji face in document */}
                <circle cx="28" cy="38" r="1.5" fill="#7C3AED" />
                <circle cx="36" cy="38" r="1.5" fill="#7C3AED" />
                <path 
                  d="M 28 45 Q 32 48 36 45" 
                  stroke="#7C3AED" 
                  strokeWidth="1.5" 
                  strokeLinecap="round"
                  fill="none"
                />

                {/* Emoji symbols around */}
                <text x="25" y="56" fontSize="8" fill="#9CA3AF">😊</text>
                <text x="38" y="56" fontSize="8" fill="#9CA3AF">❤️</text>

                {/* Small flame accent */}
                <path
                  d="M48 48 C48 48 46 51 46 53 C46 55 47 56 49 56 C51 56 52 55 52 53 C52 52 51 51 51 51 C51 51 50 52 50 52 C50 51 51 49 51 48 C51 48 50 50 50 51 C50 50 48 48 48 48Z"
                  fill="url(#emoji-flame)"
                />
              </g>

              <text x="65" y="40" fontFamily="Arial, sans-serif" fontSize="26" fontWeight="bold" fill="#1F2937">
                Blazor<tspan fill="#7C3AED">Emoji</tspan>
              </text>

              {/* Separator line */}
              <line x1="65" y1="46" x2="195" y2="46" stroke="#D1D5DB" strokeWidth="1" />

              <text x="65" y="58" fontFamily="Arial, sans-serif" fontSize="11" fontStyle="italic" fill="#6B7280">
                Emoji Picker Component
              </text>

              <defs>
                <linearGradient id="emoji-flame" x1="49" y1="48" x2="49" y2="56" gradientUnits="userSpaceOnUse">
                  <stop offset="0%" stopColor="#FBBF24" />
                  <stop offset="100%" stopColor="#F59E0B" />
                </linearGradient>
              </defs>
            </svg>             
            """;   }

    /// <summary>
    /// Generates the BlazorEmo badge SVG with the specified size.
    /// </summary>
    /// <param name="size">The width and height of the SVG in pixels (default: 80).</param>
    /// <returns>An SVG string representing the BlazorEmo badge.</returns>
    public static string GetBadgeSvg(int size = 80)
    {

        return $$$"""
             <svg width={size} height={size} viewBox="0 0 80 80" fill="none" xmlns="http://www.w3.org/2000/svg">
              {/* Rounded square background */}
              <rect x="8" y="8" width="64" height="64" rx="12" fill="url(#icon-bg)" stroke="#5B21B6" strokeWidth="2.5" />

              {/* Document icon */}
              <g>
                <path
                  d="M25 22 L25 58 L55 58 L55 35 L46 22 Z"
                  fill="white"
                  stroke="white"
                  strokeWidth="1.5"
                />
                <path 
                  d="M46 22 L46 35 L55 35" 
                  fill="#E9D5FF" 
                  stroke="white" 
                  strokeWidth="1.5" 
                  strokeLinejoin="round" 
                />

                {/* Large emoji face */}
                <circle cx="33" cy="40" r="2.5" fill="#7C3AED" />
                <circle cx="47" cy="40" r="2.5" fill="#7C3AED" />
                <path 
                  d="M 33 50 Q 40 55 47 50" 
                  stroke="#7C3AED" 
                  strokeWidth="2.5" 
                  strokeLinecap="round"
                  fill="none"
                />

                {/* Small flame accent */}
                <path
                  d="M50 50 C50 50 47 54 47 57 C47 60 49 62 52 62 C55 62 57 60 57 57 C57 55 55 54 55 54 C55 54 54 55 53 56 C53 54 55 51 55 50 C55 50 53 53 53 55 C53 53 50 50 50 50Z"
                  fill="#FCD34D"
                />
              </g>

              <defs>
                <linearGradient id="icon-bg" x1="40" y1="8" x2="40" y2="72" gradientUnits="userSpaceOnUse">
                  <stop offset="0%" stopColor="#A78BFA" />
                  <stop offset="100%" stopColor="#7C3AED" />
                </linearGradient>
              </defs>
            </svg>
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
 
}
