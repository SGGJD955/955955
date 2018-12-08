using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace Angels.Application.TicketEntity.Common.WebmapDownloader
{
    public class CC
    {
        [DllImport("classification_dll.dll", EntryPoint = "createClassifier", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr createClassifier(string prototxt_file, string caffemodel_file, float scale_raw = 1, string mean_file = null, int num_means = 0, float[] means = null, int gpu_id = -1);

        [DllImport("classification_dll.dll", EntryPoint = "predictSoftmax", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr predictSoftmax(IntPtr classifier, byte[] img, int len = 1, int top_n = 1);

        [DllImport("classification_dll.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr extfeature(IntPtr classifier, byte[] img, int len, string feature_name);

        [DllImport("classification_dll.dll", EntryPoint = "forward", CallingConvention = CallingConvention.StdCall)]
        public static extern void forward(IntPtr classifier, byte[] img, int len);

        [DllImport("classification_dll.dll", EntryPoint = "getMultiLabel", CallingConvention = CallingConvention.StdCall)]
        public static extern void getMultiLabel(IntPtr softmax, int[] buf);

        [DllImport("classification_dll.dll", EntryPoint = "getBlobData", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr getBlobData(IntPtr classifier, string blob_name);

        [DllImport("classification_dll.dll", EntryPoint = "getBlobLength", CallingConvention = CallingConvention.StdCall)]
        public static extern int getBlobLength(IntPtr feature);

        [DllImport("classification_dll.dll", EntryPoint = "getBlobDims", CallingConvention = CallingConvention.StdCall)]
        public static extern void getBlobDims(IntPtr blob, int[] dims_at_4_elem);

        [DllImport("classification_dll.dll", EntryPoint = "cpyBlobData", CallingConvention = CallingConvention.StdCall)]
        public static extern void cpyBlobData(float[] buffer, IntPtr feature);

        [DllImport("classification_dll.dll", EntryPoint = "releaseClassifier", CallingConvention = CallingConvention.StdCall)]
        public static extern void releaseClassifier(IntPtr model);

        [DllImport("classification_dll.dll", EntryPoint = "releaseSoftmaxResult", CallingConvention = CallingConvention.StdCall)]
        public static extern void releaseSoftmaxResult(IntPtr softmax);

        [DllImport("classification_dll.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void releaseBlobData(IntPtr BlobData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate int TraindEventCallback(int eventFlag, int param1, float param2, void* param3);


        [DllImport("classification_dll.dll", EntryPoint = "setTraindEventCallback", CallingConvention = CallingConvention.StdCall)]
        public static extern void setTraindEventCallback(TraindEventCallback callback);

        [DllImport("classification_dll.dll", EntryPoint = "train_network", CallingConvention = CallingConvention.StdCall)]
        public static extern int train_network(string param);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate int ConvertImageSetEventCallback(int eventFlag, int param1, float param2, void* param3);

        [DllImport("classification_dll.dll", EntryPoint = "convert_imageset", CallingConvention = CallingConvention.StdCall)]
        public static extern int convert_imageset(string param, ConvertImageSetEventCallback callback);
    }
}
