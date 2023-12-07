using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azure.ImagingSystem
{
    public enum ImagingType
    {
        None,
        Fluorescence,
        Chemiluminescence,
        PhosphorImaging,
        Visible
    }

    public class ImagingSettings
    {
        public ImagingType ImagingTabType { get; set; }
        public bool IsVisible { get; set; }


        public ImagingSettings()
        {
        }

        public ImagingSettings(ImagingType imagingType, bool bIsVisible)
        {
            this.ImagingTabType = imagingType;
            this.IsVisible = bIsVisible;
        }
    }
}
