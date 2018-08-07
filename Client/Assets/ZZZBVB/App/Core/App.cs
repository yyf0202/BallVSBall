using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighFive;

namespace BVB
{
    public class App : MonoSingleton<App>
    {
        private List<IModule> m_moduleList = new List<IModule>();

        #region life cycle

        private void Awake()
        {

        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        private void LateUpdate()
        {

        }

        private void FixedUpdate()
        {

        }

        private void OnApplicationQuit()
        {
            foreach (var module in m_moduleList)
            {
                module.UnloadModule();
            }
        }

        #endregion

        #region basics

        private void RegisterModule(IModule module)
        {
            if (m_moduleList.Contains(module))
                return;

            m_moduleList.Add(module);
            module.LoadModule();
        }


        #endregion
    }
}