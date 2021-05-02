using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Targeting
{
    [Flags]
    public enum Affiliation
    {
        PLAYERS = 1 << 0,
        ENEMIES = 1 << 1,
        NEUTRAL = 1 << 2,
        NONE = 1 << 3,
        MAX = 1 << 4,
    }


    public delegate bool TargetDiscriminatorDelegate(ITargetable potentia);

    public interface ITargetable
    {
        Affiliation TargetAffiliation
        {
            get;
            set;
        }
        int Priority
        {
            get;
        }
        GameObject Attached
        {
            get;
        }
    }
}
