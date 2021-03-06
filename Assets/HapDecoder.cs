using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Klak.Hap
{
    class HapDecoder : MonoBehaviour
    {
        IntPtr _hap;
        Texture2D _texture;

        void Start()
        {
            _hap = HapOpen("Assets\\StreamingAssets\\Test.mov");

            _texture = new Texture2D(
                HapGetVideoWidth(_hap),
                HapGetVideoHeight(_hap),
                TextureFormat.DXT1,
                false
            );
        }

        void OnDestroy()
        {
            if (_hap != IntPtr.Zero)
            {
                HapClose(_hap);
                _hap = IntPtr.Zero;
            }

            Destroy(_texture);
        }

        unsafe void Update()
        {
            var time = 1 - Mathf.Abs(1 - Time.time / 3 % 1 * 2);
            var index = (int)(time * HapCountFrames(_hap));
            
            HapDecodeFrame(_hap, index);

            var data = new NativeArray<byte>((int)HapGetBufferSize(_hap), Allocator.Temp);

            UnsafeUtility.MemCpy
                (data.GetUnsafePtr(), (void*)HapGetBufferPointer(_hap), data.Length);

            _texture.LoadRawTextureData(data);
            _texture.Apply();

            GetComponent<Renderer>().material.mainTexture = _texture;
        }

        [DllImport("KlakHap")]
        internal static extern IntPtr HapOpen(string filepath);

        [DllImport("KlakHap")]
        internal static extern void HapClose(IntPtr context);

        [DllImport("KlakHap")]
        internal static extern long HapCountFrames(IntPtr context);

        [DllImport("KlakHap")]
        internal static extern int HapGetVideoWidth(IntPtr context);

        [DllImport("KlakHap")]
        internal static extern int HapGetVideoHeight(IntPtr context);

        [DllImport("KlakHap")]
        internal static extern void HapDecodeFrame(IntPtr context, int index);

        [DllImport("KlakHap")]
        internal static extern IntPtr HapGetBufferPointer(IntPtr context);

        [DllImport("KlakHap")]
        internal static extern long HapGetBufferSize(IntPtr context);
    }
}
