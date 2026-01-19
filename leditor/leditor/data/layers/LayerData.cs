using Newtonsoft.Json;

namespace leditor.root;

public class LayerData
{
    [JsonProperty] public LayerID[] Layers;

    private readonly Dictionary<string, LayerID> stringIDToLayer = new();

    public LayerData(LayerID[] layers)
    {
        Layers = layers;
    }

    public void ChangeLayers(string[] newLayers)
    {
        Layers = new LayerID[newLayers.Length];
        for (var i = 0; i < newLayers.Length; i++)
        {
            var layer = newLayers[i];
            Layers[i] = new LayerID(layer);
        }
    }
}