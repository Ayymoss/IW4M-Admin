﻿namespace IW4MAdmin.Plugins.LiveRadar;

public class MapInfo
{
    public string Name { get; set; }
    public string Alias { get; set; }
    // distance from the edge of the minimap image
    // to the "playable" area
    public int Top { get; set; }
    public int Bottom { get; set; }
    public int Left { get; set; }
    public int Right { get; set; }
    // maximum coordinate values for the map
    public int MaxTop { get; set; }
    public int MaxBottom { get; set; }
    public int MaxLeft { get; set; }
    public int MaxRight { get; set; }
    public float Rotation { get; set; }
    public float ViewPositionRotation { get; set; }
    public float CenterX { get; set; }
    public float CenterY { get; set; }
    public float Scaler { get; set; } = 1.0f;

    public int Width => MaxLeft - MaxRight;
    public int Height => MaxTop - MaxBottom;
}
