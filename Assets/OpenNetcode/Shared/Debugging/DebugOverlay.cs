using OpenNetcode.Shared.Time;
using SourceConsole.UI;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Debugging
{
    public struct TickElement
    {
        public int Tick;
        public Color Color;
    }
    
    public class DebugOverlay : MonoBehaviour
    {
        private static DebugOverlay _instance;

        public static void AddTickElement(FixedString32Bytes label, TickElement element)
        {
            _instance._elements[label] = element;
        }
        
        internal NativeHashMap<FixedString32Bytes, TickElement> _elements;
        
        private bool _toggleView;
        private Entity clientEntity;

        private Texture2D _white;

        private void OnDestroy()
        {
            _elements.Dispose();
        }

        void Awake()
        {
            _instance = this;
            _elements = new NativeHashMap<FixedString32Bytes, TickElement>(100, Allocator.Persistent);

            _white = new Texture2D(1, 1);
            _white.SetPixel(0,0, Color.white);
            _white.Apply();

        }
        
        void OnGUI()
        {
            if (!ConsoleCanvasController.IsVisible())
                return;
            
            Rect elementsRect = new Rect(20, 20, 500, 200);

            float labelWidth = 150;
            float height = 20;

            int areaWidth = TimeConfig.TicksPerSecond * 5;

            int i = 0;
            foreach(var element in _elements)
            {
                i++;
                // Label
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(elementsRect.x, elementsRect.y + height * i, labelWidth, height), _white);
                GUI.color = Color.black;
                GUI.Label(new Rect(elementsRect.x, elementsRect.y + height * i, labelWidth, height), element.Key.ToString());
                
                // Area background
                GUI.color = new Color(0.9f, 0.9f, 0.9f);
                GUI.DrawTexture(new Rect(elementsRect.x + labelWidth, elementsRect.y + height * i, areaWidth, height), _white);
                
                // Area separators
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(elementsRect.x + labelWidth, elementsRect.y + height * i, areaWidth, 1), _white);
                GUI.DrawTexture(new Rect(elementsRect.x + labelWidth, elementsRect.y + height * i + height, areaWidth, 1), _white);
                
                // Tick
                GUI.color = element.Value.Color;
                GUI.Label(new Rect(elementsRect.x + labelWidth + element.Value.Tick % areaWidth, elementsRect.y + height * i, areaWidth, height), "|");
            }
        }
    }
}