using UnityEngine;
using System.Collections;

public class MathHelpers {

    public static bool RectanglesOverlap(Rect rectangle1, Rect rectangle2, int offset)
    {
        float xMin1 = rectangle1.x - offset;
        float xMax1 = rectangle1.x + rectangle1.width + offset;
        float yMin1 = rectangle1.y - offset;
        float yMax1 = rectangle1.y + rectangle1.height + offset;

        float xMin2 = rectangle2.x - offset;
        float xMax2 = rectangle2.x + rectangle2.width + offset;
        float yMin2 = rectangle2.y - offset;
        float yMax2 = rectangle2.y + rectangle2.height + offset;

        if (xMin1< xMax2
            && xMax1 > xMin2 
            && yMin1 < yMax2
            && yMax1 > yMin2)
        {
            return true;
        }

        return false;
    }
}
