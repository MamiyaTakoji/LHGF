using UnityEngine;


namespace TexDrawLib
{
    /// Interface that implemented for any TEXDraw component
    public interface ITEXDraw
    {
        /// Text to display in $\LaTeX$ 
        string text { get; set; }

        /// Initial text size in pixels, or world unit
        float size { get; set; }

        /// Initial font color in RGBA `Color`
        Color color { get; set; }

        /// Initial alignment to set, in code the value is in `Vector2`
        Vector2 alignment { get; set; }

        /// Display offset in pixels, or world unit
        Rect scrollArea { get; set; }

        /// Inset padding in pixels, or world unit
        TexRectOffset padding { get; set; }

        /// Overflow method to use
        Overflow overflow { get; set; }

        /// Access for internal Cached Preference
        TEXPreference preference { get; }

        /// Access to internal Orchestrator Engine
        TexOrchestrator orchestrator { get; }

        /// Will Trigger reparsing
        void SetTextDirty();
    }

    /// Interface that implemented for any TEXDraw renderer
    public interface ITexRenderer
    {
        /// Access to parent TEXDraw component
        ITEXDraw TEXDraw { get; }

        /// Get or set font responsible for rendering
        /// Note: some sentinel modes that used:
        /// -1: Dead, -2: Front Render, -3: Back Render
        int FontMode { get; set; }
    }
}
