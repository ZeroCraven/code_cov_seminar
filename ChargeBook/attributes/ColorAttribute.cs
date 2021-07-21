using System;
using System.Drawing;
using System.Reflection;
using chargebook.models;

namespace chargebook.attributes {
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class ColorAttribute : Attribute {
        public string brightColorHex { get; }
        public string darkColorHex { get; }

        public ColorAttribute(string brightColorHex, string darkColorHex) {
            this.brightColorHex = brightColorHex;
            this.darkColorHex = darkColorHex;
        }
    }
}