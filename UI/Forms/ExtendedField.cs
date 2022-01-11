using System;

namespace ServerBrowser.UI.Forms
{
    public abstract class ExtendedField
    {
        /// <summary>
        /// Toggles whether this field should be visible or not.
        /// </summary>
        public abstract bool Visible { get; set; }
        
        /// <summary>
        /// Gets the field height for vertical layout calculations.
        /// </summary>
        public abstract float Height { get; }
    }
    
    public abstract class ExtendedField<TValue> : ExtendedField
    {
        /// <summary>
        /// Gets or sets the value on the form field.
        /// </summary>
        public abstract TValue Value { get; set; }
        
        /// <summary>
        /// This event is triggered when the user changes the field value.
        /// </summary>
        public abstract event EventHandler<TValue>? OnChange;
    }
}