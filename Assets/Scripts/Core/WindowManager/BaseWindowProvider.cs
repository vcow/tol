using System.Collections.Generic;
using UnityEngine;

namespace Core.WindowManager
{
    public abstract class BaseWindowProvider : ScriptableObject
    {
        public abstract IReadOnlyList<WindowImpl> Windows { get; }
    }
}