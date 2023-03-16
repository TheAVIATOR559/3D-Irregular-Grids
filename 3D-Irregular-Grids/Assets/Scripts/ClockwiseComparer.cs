using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//provided by https://answers.unity.com/questions/877169/vector2-array-sort-clockwise.html

public class ClockwiseComparer : IComparer<Vector3>
{
    private Vector3 m_Origin;

    #region Properties

    /// <summary>
    ///     Gets or sets the origin.
    /// </summary>
    /// <value>The origin.</value>
    public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }

    #endregion

    /// <summary>
    ///     Initializes a new instance of the ClockwiseComparer class.
    /// </summary>
    /// <param name="origin">Origin.</param>
    public ClockwiseComparer(Vector3 origin)
    {
        m_Origin = origin;
    }

    #region IComparer Methods

    /// <summary>
    ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    public int Compare(Vector3 first, Vector3 second)
    {
        return IsClockwise(first, second, m_Origin);
    }

    #endregion

    /// <summary>
    ///     Returns 1 if first comes before second in clockwise order.
    ///     Returns -1 if second comes before first.
    ///     Returns 0 if the points are identical.
    /// </summary>
    /// <param name="first">First.</param>
    /// <param name="second">Second.</param>
    /// <param name="origin">Origin.</param>
    public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
    {
        if (first == second)
            return 0;

        Vector3 firstOffset = first - origin;
        Vector3 secondOffset = second - origin;

        float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.z);
        float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.z);

        if (angle1 < angle2)
            return -1;

        if (angle1 > angle2)
            return 1;

        // Check to see which point is closest
        return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
    }
}