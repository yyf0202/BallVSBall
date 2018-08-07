using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BVB
{
    public interface IModule
    {
        void LoadModule();
        void UnloadModule();
    }

    public interface IModuleUpdate
    {
        void UpdateModule();
    }

    public interface IModuleLateUpdate
    {
        void LateUpdateModule();
    }

    public interface IModuleFixedUpdate
    {
        void FixedUpdateModule();
    }
}