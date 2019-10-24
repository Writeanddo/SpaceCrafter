using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static List<Vector2> CaridanalPoints
    {
        get {
            return new List<Vector2>()
                {
                    Vector2.up,
                    Vector2.left,
                    Vector2.down,
                    Vector2.right,
                };
        }
    }
}