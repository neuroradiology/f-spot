using System;

namespace FSpot.Gui.MetadataEditor
{
    public interface IFieldEditor
    {
        void UpdateEditor (EditorPhoto info);
        void UpdateInfo (EditorPhoto info);
    }
}

