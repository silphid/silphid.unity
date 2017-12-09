using System.IO;
using log4net.ObjectRenderer;
using UnityEngine;
using SystemInfo = log4net.Util.SystemInfo;

namespace log4net.Unity
{
    public class UnityObjectPair : IObjectRenderer
    {
        private readonly string m_message;
        private readonly Object m_unityObject;

        public UnityObjectPair(string message, Object unityObject)
        {
            m_message = message;
            m_unityObject = unityObject;
        }

        public Object UnityObject
        {
            get { return m_unityObject; }
        }

        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            if (obj == null)
            {
                writer.Write(SystemInfo.NullText);
                return;
            }
            UnityObjectPair unityObjectPair = obj as UnityObjectPair;

            if (unityObjectPair == null)
            {
                rendererMap.FindAndRender(obj, writer);
                return;
            }

            writer.Write(unityObjectPair.m_message ?? SystemInfo.NullText);
        }
    }
}