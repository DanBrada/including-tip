using BlazorLeafletInterop.Models.Options.Layer.Raster;

namespace IncludingTip.Client;

/// <summary>
/// Just TileLayerOptions that add Attribution, just to satisfy OSM licensing, because the interop doesn't include it somehow.
/// </summary>
public class BetterTileLayerOptions: TileLayerOptions
{
    public string Attribution { get; set; }=
        """&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>""";

}