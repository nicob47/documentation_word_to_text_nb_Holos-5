using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Avalonia.Models.Results
{
    public class EmissionPieChartViewItem
    {
        public string GroupName { get; set; }
        public string EmissionType { get; set; }
        public double Value { get; set; }
        public string Label { get; set; }
    }
}
