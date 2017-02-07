using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace plugins2
{
    class Signal : ICloneable
    {
        public float[] values;

        public float[] defaultValues;

        public float Scale_Y { get; set; }

        public float Vertical_position { get; set; }

        public float Addition_to_signal { get; set; }

        public PointF FactorXandY { get; set; }

        public int num_of_missing_signal { get; set; }

        public bool drawing_allowed { get; set; }

        public bool computed_all { get; set; }

        public float[] viewSignal;

        public PointF[] pointsF { get; set; }

        public bool[] PointInMark;

        public Signal(float scale_y, float vertical_position, bool allow_drawing)
        {
            Scale_Y = scale_y;
            Vertical_position = vertical_position;
            drawing_allowed = allow_drawing;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        //public PointF returnFactor()
        //{
        //    return this.FactorXandY;
        //}
        //public float returnAddition()
        //{
        //    return this.Addition_to_signal;
        //}

    }
}
