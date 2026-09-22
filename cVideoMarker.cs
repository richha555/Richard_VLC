using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LibVLCSharp.Shared;

namespace Richard_VLC
{
    public enum eStartStop { BEGIN = 0, END = 1, NONE = 2 }
    public class cVideoMarker
    {
        public Guid MarkerGUID { get; set; }
        public int MarkerID { get; set; }
        public double Position { get; set; }                 // [0]
        public TimeSpan Offset { get; set; }                 // [1]
        public string Title { get; set; }                    // [2]
        public string Description { get; set; }              // [3]
        public Color Color { get; set; }                     // [4]

        public eStartStop StartStop { get; set; } = eStartStop.NONE;  // [5]
        public Label Label { get; set; }
        public bool ParseOffset(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            string[] parts = value.Split(':');

            int hours = -1;
            int minutes = -1;
            double secs = -1.0;

            if (parts.Length == 1) {
                // Seconds, possibly with milliseconds
                if (!double.TryParse(parts[0], out secs)) {
                    return false;
                }
                this.Offset = TimeSpan.FromSeconds(secs);
                return true;
            }

            if (parts.Length == 2) {
                // Minutes : Seconds
                if (!int.TryParse(parts[0], out minutes)) {
                    return false;
                }
                if (!double.TryParse(parts[1], out secs)) {
                    return false;
                }
                this.Offset = TimeSpan.FromMinutes(minutes) +
                              TimeSpan.FromSeconds(secs);
                return true;
            }

            if (parts.Length == 3) {
                // Hours : Minutes : Seconds
                if (!int.TryParse(parts[0], out hours)) {
                    return false;
                }
                if (!int.TryParse(parts[1], out minutes)) {
                    return false;
                }
                if (!double.TryParse(parts[2], out secs)) {
                    return false;
                }

                this.Offset = TimeSpan.FromHours(hours) +
                              TimeSpan.FromMinutes(minutes) +
                              TimeSpan.FromSeconds(secs);
            }

            return false;
        //  throw new FormatException("Invalid time format: " + value);
        }

        public bool ParseColor(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try {
                Color clr = Color.FromName(value);
                this.Color = clr;
            } catch (Exception ex) {
                return false;
            }
            return true;
        }
    }

    public class cVideoMarkers
    {
        public List<cVideoMarker> markers { get; set; } = new();

        public cVideoMarker? curr_marker { get; set; } = null;

        public int max_id { get; set; } = 0;

        public cVideoMarker Add_Marker(double Position, TimeSpan Offset, string Title, string Description, Color Color, Label Label)
        {
            cVideoMarker marker = new cVideoMarker();

            max_id += 1;

            marker.MarkerGUID = Guid.NewGuid();

            marker.MarkerID = max_id;

            marker.Position = Position;
            marker.Offset = Offset;
            marker.Title = string.IsNullOrWhiteSpace(Title) ? string.Format("{0:000}", max_id) : Title;
            marker.Description = Description;
            marker.Color = Color;
            marker.Label = Label;

            markers.Add(marker);

            Rearrange_Markers();

            return marker;
        }

        public void Rearrange_Markers()
        {
            markers.Sort((x, y) => x.Position.CompareTo(y.Position));  // auomatically sort list by FrameNum

            int n = 0;
            foreach (cVideoMarker m in markers) {
                n += 1;
                m.MarkerID = n;
                if (Regex.IsMatch(m.Title, @"^[0-9]{3}$")) {
                    // Title was Merker-Number...  rename it
                    m.Title = string.Format("{0:000}", n);
                }
            }
        }

        public cVideoMarker? Select_Marker(double Position)
        {
            cVideoMarker? marker = this.markers.FirstOrDefault(x => Math.Abs(x.Position - Position) < 0.5);

            if (marker == null || marker == default(cVideoMarker)) return null;

            this.curr_marker = marker;

            return marker;
        }
    }
    public static class AppConstants
    {
        public static readonly IReadOnlyList<string> MarkerColors =
            new[]
            {
                "LightPink",
                "HotPink",
                "DeepPink",
                "Crimson",
                "Red",
                "Orange",
                "DarkOrange",
                "OrangeRed",
                "Gold",
                "Yellow",
                "LawnGreen",
                "Lime",
                "LimeGreen",
                "SeaGreen",
                "Olive",
                "DarkGreen",
                "Cyan",
                "DarkTurquoise",
                "LightSkyBlue",
                "CornflowerBlue",
                "Blue",
                "DarkBlue",
                "Tan",
                "BurlyWood",
                "SaddleBrown",
                "Sienna",
                "Magenta",
                "MediumOrchid",
                "DarkViolet",
                "Purple",
                "Silver",
                "DimGray",
                "DarkSlateGray",            
            };
    }
    //public interface IEditorHost
    //{
    //    private cVideoMarker Editor_NewMarker_at_CurrPos(object sender, EventArgs e);
    //    private void Editor_Move_Marker_Left_1(object sender, EventArgs e);
    //    private void Editor_Move_Marker_Left_1(object sender, EventArgs e);
    //    private void Editor_Move_Marker_Right_1(object sender, EventArgs e);
    //    private void Editor_Remove_Marker(object sender, EventArgs e);
    //    private void Editor_GoTo_Marker(object sender, EventArgs e);
    //}
}
