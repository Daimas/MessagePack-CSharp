#if UNITY_EDITOR
using System;
using System.Threading;
using MessagePackCompiler;
using UnityEditor;
using UnityEngine;

namespace MessagePack.Unity.Editor
{
    public static class MessagePackEditorSetup
    {
        [MenuItem("Tools/MessagePack/Generate Classes")]
        public static void GenerateClasses()
        {
            Generate();
        }

        private static async void Generate()
        {
            try
            {
                await new CodeGenerator(s => Debug.Log(s), new CancellationToken())
                    .GenerateFileAsync(@"Assets\MessagePackGenerated\", "GeneratedResolver", "MessagePack", false, null, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif
