module SpiralExample.Main
let cuda_kernels = """
extern "C" {
    typedef long long int(*FunPointer0)(long long int, long long int);
    __global__ void method_7(long long int * var_0, long long int * var_1);
    __device__ void method_8(long long int * var_0, long long int * var_1, long long int var_2);
    __device__ long long int method_6(long long int var_0, long long int var_1);
    
    __global__ void method_7(long long int * var_0, long long int * var_1) {
        long long int var_2 = threadIdx.x;
        long long int var_3 = threadIdx.y;
        long long int var_4 = threadIdx.z;
        long long int var_5 = blockIdx.x;
        long long int var_6 = blockIdx.y;
        long long int var_7 = blockIdx.z;
        long long int var_8 = (var_5 * 128);
        long long int var_9 = (var_8 + var_2);
        method_8(var_0, var_1, var_9);
    }
    __device__ void method_8(long long int * var_0, long long int * var_1, long long int var_2) {
        if ((var_2 < 8)) {
            long long int var_3 = var_1[var_2];
            FunPointer0 var_6 = method_6;
            long long int var_7 = var_6(var_3, var_3);
            var_0[var_2] = var_7;
            long long int var_8 = (var_2 + 4096);
            method_8(var_0, var_1, var_8);
        } else {
        }
    }
    __device__ long long int method_6(long long int var_0, long long int var_1) {
        return (var_0 + var_1);
    }
}
"""

type Union0 =
    | Union0Case0 of Tuple1
    | Union0Case1
and Tuple1 =
    struct
    val mem_0: ManagedCuda.BasicTypes.CUdeviceptr
    new(arg_mem_0) = {mem_0 = arg_mem_0}
    end
and EnvStack2 =
    struct
    val mem_0: (Union0 ref)
    new(arg_mem_0) = {mem_0 = arg_mem_0}
    end
and Env3 =
    struct
    val mem_0: EnvStack2
    val mem_1: int64
    new(arg_mem_0, arg_mem_1) = {mem_0 = arg_mem_0; mem_1 = arg_mem_1}
    end
and EnvStack4 =
    struct
    val mem_0: uint64
    val mem_1: uint64
    val mem_2: System.Collections.Generic.Stack<Env3>
    val mem_3: ManagedCuda.CudaContext
    new(arg_mem_0, arg_mem_1, arg_mem_2, arg_mem_3) = {mem_0 = arg_mem_0; mem_1 = arg_mem_1; mem_2 = arg_mem_2; mem_3 = arg_mem_3}
    end
and EnvHeap5 =
    {
    mem_0: (int64 [])
    }
let rec method_0 ((var_0: System.Diagnostics.DataReceivedEventArgs)): unit =
    let (var_1: string) = var_0.get_Data()
    System.Console.WriteLine(var_1)
and method_1((var_0: (Union0 ref))): ManagedCuda.BasicTypes.CUdeviceptr =
    let (var_1: Union0) = (!var_0)
    match var_1 with
    | Union0Case0(var_2) ->
        var_2.mem_0
    | Union0Case1 ->
        (failwith "A Cuda memory cell that has been disposed has been tried to be accessed.")
and method_2((var_0: (int64 [])), (var_1: int64), (var_2: int64)): int64 =
    if (var_1 <= 7L) then
        var_0.[int32 var_2] <- var_1
        let (var_3: int64) = (var_2 + 1L)
        let (var_4: int64) = (var_1 + 1L)
        method_2((var_0: (int64 [])), (var_4: int64), (var_3: int64))
    else
        var_2
and method_3((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env3>), (var_2: uint64), (var_3: int64)): EnvStack2 =
    let (var_4: int32) = var_1.get_Count()
    if (var_4 > 0) then
        let (var_5: Env3) = var_1.Peek()
        let (var_6: EnvStack2) = var_5.mem_0
        let (var_7: int64) = var_5.mem_1
        let (var_8: (Union0 ref)) = var_6.mem_0
        let (var_9: Union0) = (!var_8)
        match var_9 with
        | Union0Case0(var_10) ->
            let (var_11: ManagedCuda.BasicTypes.CUdeviceptr) = var_10.mem_0
            method_4((var_11: ManagedCuda.BasicTypes.CUdeviceptr), (var_0: uint64), (var_2: uint64), (var_3: int64), (var_1: System.Collections.Generic.Stack<Env3>), (var_6: EnvStack2), (var_7: int64))
        | Union0Case1 ->
            let (var_13: Env3) = var_1.Pop()
            let (var_14: EnvStack2) = var_13.mem_0
            let (var_15: int64) = var_13.mem_1
            method_3((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env3>), (var_2: uint64), (var_3: int64))
    else
        method_5((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env3>), (var_2: uint64), (var_3: int64))
and method_9((var_0: (int64 []))): string =
    let (var_1: System.Text.StringBuilder) = System.Text.StringBuilder()
    let (var_2: System.Text.StringBuilder) = var_1.Append("[|")
    let (var_3: int64) = var_0.LongLength
    let (var_4: int64) = 0L
    let (var_5: string) = method_10((var_0: (int64 [])), (var_1: System.Text.StringBuilder), (var_3: int64), (var_4: int64))
    let (var_6: System.Text.StringBuilder) = var_1.Append("|]")
    var_1.ToString()
and method_4((var_0: ManagedCuda.BasicTypes.CUdeviceptr), (var_1: uint64), (var_2: uint64), (var_3: int64), (var_4: System.Collections.Generic.Stack<Env3>), (var_5: EnvStack2), (var_6: int64)): EnvStack2 =
    let (var_7: ManagedCuda.BasicTypes.SizeT) = var_0.Pointer
    let (var_8: uint64) = uint64(var_7)
    let (var_9: uint64) = uint64(var_6)
    let (var_10: uint64) = (var_8 - var_1)
    let (var_11: uint64) = (var_10 + var_9)
    let (var_12: uint64) = uint64(var_3)
    let (var_13: uint64) = (var_12 + var_11)
    let (var_14: bool) = (var_13 <= var_2)
    if var_14 then
        ()
    else
        (failwith "Cache size has been exceeded in the allocator.")
    let (var_15: uint64) = (var_8 + var_9)
    let (var_16: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_15)
    let (var_17: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_16)
    let (var_18: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_17))))
    let (var_19: EnvStack2) = EnvStack2((var_18: (Union0 ref)))
    var_4.Push((Env3(var_19, var_3)))
    var_19
and method_5((var_0: uint64), (var_1: System.Collections.Generic.Stack<Env3>), (var_2: uint64), (var_3: int64)): EnvStack2 =
    let (var_4: uint64) = uint64(var_3)
    let (var_5: bool) = (var_4 <= var_2)
    if var_5 then
        ()
    else
        (failwith "Cache size has been exceeded in the allocator.")
    let (var_6: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_0)
    let (var_7: ManagedCuda.BasicTypes.CUdeviceptr) = ManagedCuda.BasicTypes.CUdeviceptr(var_6)
    let (var_8: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_7))))
    let (var_9: EnvStack2) = EnvStack2((var_8: (Union0 ref)))
    var_1.Push((Env3(var_9, var_3)))
    var_9
and method_10((var_0: (int64 [])), (var_1: System.Text.StringBuilder), (var_2: int64), (var_3: int64)): string =
    if (var_3 < var_2) then
        let (var_4: int64) = var_0.[int32 var_3]
        let (var_5: System.Text.StringBuilder) = var_1.Append("")
        let (var_6: string) = System.Convert.ToString(var_4)
        let (var_7: System.Text.StringBuilder) = var_1.Append(var_6)
        let (var_8: int64) = (var_3 + 1L)
        method_11((var_0: (int64 [])), (var_1: System.Text.StringBuilder), (var_2: int64), (var_8: int64))
    else
        ""
and method_11((var_0: (int64 [])), (var_1: System.Text.StringBuilder), (var_2: int64), (var_3: int64)): string =
    if (var_3 < var_2) then
        let (var_4: int64) = var_0.[int32 var_3]
        let (var_5: System.Text.StringBuilder) = var_1.Append("; ")
        let (var_6: string) = System.Convert.ToString(var_4)
        let (var_7: System.Text.StringBuilder) = var_1.Append(var_6)
        let (var_8: int64) = (var_3 + 1L)
        method_11((var_0: (int64 [])), (var_1: System.Text.StringBuilder), (var_2: int64), (var_8: int64))
    else
        "; "
let (var_0: string) = cuda_kernels
let (var_1: ManagedCuda.CudaContext) = ManagedCuda.CudaContext(false)
let (var_2: string) = System.Environment.get_CurrentDirectory()
let (var_3: string) = System.IO.Path.Combine(var_2, "nvcc_router.bat")
let (var_4: System.Diagnostics.ProcessStartInfo) = System.Diagnostics.ProcessStartInfo()
var_4.set_RedirectStandardOutput(true)
var_4.set_RedirectStandardError(true)
var_4.set_UseShellExecute(false)
var_4.set_FileName(var_3)
let (var_5: System.Diagnostics.Process) = System.Diagnostics.Process()
var_5.set_StartInfo(var_4)
let (var_7: (System.Diagnostics.DataReceivedEventArgs -> unit)) = method_0
var_5.OutputDataReceived.Add(var_7)
var_5.ErrorDataReceived.Add(var_7)
let (var_8: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Auxiliary\\Build\\vcvarsx86_amd64.bat")
let (var_9: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Tools\\MSVC\\14.11.25503\\bin\\Hostx64\\x64\\cl.exe")
let (var_10: string) = System.IO.Path.Combine("C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v9.0", "include")
let (var_11: string) = System.IO.Path.Combine("C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community", "VC\\Tools\\MSVC\\14.11.25503\\include")
let (var_12: string) = System.IO.Path.Combine("C:\\Program Files\\NVIDIA GPU Computing Toolkit\\CUDA\\v9.0", "bin\\nvcc.exe")
let (var_13: string) = System.IO.Path.Combine(var_2, "cuda_kernels.ptx")
let (var_14: string) = System.IO.Path.Combine(var_2, "cuda_kernels.cu")
let (var_15: bool) = System.IO.File.Exists(var_14)
if var_15 then
    System.IO.File.Delete(var_14)
else
    ()
System.IO.File.WriteAllText(var_14, var_0)
let (var_16: bool) = System.IO.File.Exists(var_3)
if var_16 then
    System.IO.File.Delete(var_3)
else
    ()
let (var_17: System.IO.FileStream) = System.IO.File.OpenWrite(var_3)
let (var_18: System.IO.Stream) = (var_17 :> System.IO.Stream)
let (var_19: System.IO.StreamWriter) = System.IO.StreamWriter(var_18)
let (var_20: int64) = (int64 var_8.Length)
let (var_21: int64) = (17L + var_20)
let (var_22: int32) = (int32 var_21)
let (var_23: System.Text.StringBuilder) = System.Text.StringBuilder(var_22)
let (var_24: System.Text.StringBuilder) = var_23.Append("call ")
let (var_25: System.Text.StringBuilder) = var_24.Append('"')
let (var_26: System.Text.StringBuilder) = var_25.Append(var_8)
let (var_27: System.Text.StringBuilder) = var_26.Append('"')
let (var_28: string) = var_27.ToString()
var_19.WriteLine(var_28)
let (var_29: int64) = (int64 var_12.Length)
let (var_30: int64) = (int64 var_9.Length)
let (var_31: int64) = (var_29 + var_30)
let (var_32: int64) = (int64 var_10.Length)
let (var_33: int64) = (var_31 + var_32)
let (var_34: int64) = (int64 var_11.Length)
let (var_35: int64) = (var_33 + var_34)
let (var_36: int64) = (int64 var_2.Length)
let (var_37: int64) = (var_35 + var_36)
let (var_38: int64) = (int64 var_13.Length)
let (var_39: int64) = (var_37 + var_38)
let (var_40: int64) = (int64 var_14.Length)
let (var_41: int64) = (var_39 + var_40)
let (var_42: int64) = (283L + var_41)
let (var_43: int32) = (int32 var_42)
let (var_44: System.Text.StringBuilder) = System.Text.StringBuilder(var_43)
let (var_45: System.Text.StringBuilder) = var_44.Append('"')
let (var_46: System.Text.StringBuilder) = var_45.Append(var_12)
let (var_47: System.Text.StringBuilder) = var_46.Append('"')
let (var_48: System.Text.StringBuilder) = var_47.Append(" -gencode=arch=compute_30,code=\\\"sm_30,compute_30\\\" --use-local-env --cl-version 2017 -ccbin ")
let (var_49: System.Text.StringBuilder) = var_48.Append('"')
let (var_50: System.Text.StringBuilder) = var_49.Append(var_9)
let (var_51: System.Text.StringBuilder) = var_50.Append('"')
let (var_52: System.Text.StringBuilder) = var_51.Append(" -I")
let (var_53: System.Text.StringBuilder) = var_52.Append('"')
let (var_54: System.Text.StringBuilder) = var_53.Append(var_10)
let (var_55: System.Text.StringBuilder) = var_54.Append('"')
let (var_56: System.Text.StringBuilder) = var_55.Append(" -I")
let (var_57: System.Text.StringBuilder) = var_56.Append('"')
let (var_58: System.Text.StringBuilder) = var_57.Append("C:\\cub-1.7.4")
let (var_59: System.Text.StringBuilder) = var_58.Append('"')
let (var_60: System.Text.StringBuilder) = var_59.Append(" -I")
let (var_61: System.Text.StringBuilder) = var_60.Append('"')
let (var_62: System.Text.StringBuilder) = var_61.Append(var_11)
let (var_63: System.Text.StringBuilder) = var_62.Append('"')
let (var_64: System.Text.StringBuilder) = var_63.Append(" --keep-dir ")
let (var_65: System.Text.StringBuilder) = var_64.Append('"')
let (var_66: System.Text.StringBuilder) = var_65.Append(var_2)
let (var_67: System.Text.StringBuilder) = var_66.Append('"')
let (var_68: System.Text.StringBuilder) = var_67.Append(" -maxrregcount=0  --machine 64 -ptx -cudart static  -o ")
let (var_69: System.Text.StringBuilder) = var_68.Append('"')
let (var_70: System.Text.StringBuilder) = var_69.Append(var_13)
let (var_71: System.Text.StringBuilder) = var_70.Append('"')
let (var_72: System.Text.StringBuilder) = var_71.Append(' ')
let (var_73: System.Text.StringBuilder) = var_72.Append('"')
let (var_74: System.Text.StringBuilder) = var_73.Append(var_14)
let (var_75: System.Text.StringBuilder) = var_74.Append('"')
let (var_76: string) = var_75.ToString()
var_19.WriteLine(var_76)
var_19.Dispose()
var_17.Dispose()
let (var_77: bool) = var_5.Start()
if (var_77 = false) then
    (failwith "NVCC failed to run.")
else
    ()
var_5.BeginOutputReadLine()
var_5.BeginErrorReadLine()
var_5.WaitForExit()
let (var_78: int32) = var_5.get_ExitCode()
if (var_78 <> 0) then
    let (var_79: System.Text.StringBuilder) = System.Text.StringBuilder(40)
    let (var_80: System.Text.StringBuilder) = var_79.Append("NVCC failed compilation with code ")
    let (var_81: System.Text.StringBuilder) = var_80.Append(var_78)
    let (var_82: string) = var_81.ToString()
    (failwith var_82)
else
    ()
let (var_83: ManagedCuda.BasicTypes.CUmodule) = var_1.LoadModulePTX(var_13)
var_5.Dispose()
let (var_84: int64) = (51L + var_36)
let (var_85: int32) = (int32 var_84)
let (var_86: System.Text.StringBuilder) = System.Text.StringBuilder(var_85)
let (var_87: System.Text.StringBuilder) = var_86.Append("Compiled the kernels into the following directory: ")
let (var_88: System.Text.StringBuilder) = var_87.Append(var_2)
let (var_89: string) = var_88.ToString()
System.Console.WriteLine(var_89)
let (var_90: ManagedCuda.CudaDeviceProperties) = var_1.GetDeviceInfo()
let (var_91: ManagedCuda.BasicTypes.SizeT) = var_90.get_TotalGlobalMemory()
let (var_92: int64) = int64(var_91)
let (var_93: float) = Microsoft.FSharp.Core.Operators.float(var_92)
let (var_94: float) = (0.700000 * var_93)
let (var_95: int64) = int64(var_94)
let (var_96: ManagedCuda.BasicTypes.SizeT) = ManagedCuda.BasicTypes.SizeT(var_95)
let (var_97: ManagedCuda.BasicTypes.CUdeviceptr) = var_1.AllocateMemory(var_96)
let (var_98: (Union0 ref)) = (ref (Union0Case0(Tuple1(var_97))))
let (var_99: EnvStack2) = EnvStack2((var_98: (Union0 ref)))
let (var_100: System.Collections.Generic.Stack<Env3>) = System.Collections.Generic.Stack<Env3>()
let (var_101: (Union0 ref)) = var_99.mem_0
let (var_102: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_101: (Union0 ref)))
let (var_103: ManagedCuda.BasicTypes.SizeT) = var_102.Pointer
let (var_104: uint64) = uint64(var_103)
let (var_105: uint64) = uint64(var_95)
let (var_106: EnvStack4) = EnvStack4((var_104: uint64), (var_105: uint64), (var_100: System.Collections.Generic.Stack<Env3>), (var_1: ManagedCuda.CudaContext))
let (var_107: uint64) = var_106.mem_0
let (var_108: uint64) = var_106.mem_1
let (var_109: System.Collections.Generic.Stack<Env3>) = var_106.mem_2
let (var_110: ManagedCuda.CudaContext) = var_106.mem_3
let (var_111: (int64 [])) = Array.zeroCreate<int64> (System.Convert.ToInt32(8L))
let (var_112: int64) = 0L
let (var_113: int64) = 0L
let (var_114: int64) = method_2((var_111: (int64 [])), (var_113: int64), (var_112: int64))
let (var_115: EnvHeap5) = ({mem_0 = (var_111: (int64 []))} : EnvHeap5)
let (var_116: (int64 [])) = var_115.mem_0
let (var_117: int64) = var_116.LongLength
let (var_118: int64) = (int64 sizeof<int64>)
let (var_119: int64) = (var_117 * var_118)
let (var_120: EnvStack2) = method_3((var_107: uint64), (var_109: System.Collections.Generic.Stack<Env3>), (var_108: uint64), (var_119: int64))
let (var_121: (Union0 ref)) = var_120.mem_0
let (var_122: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_121: (Union0 ref)))
var_110.CopyToDevice(var_122, var_116)
let (var_127: int64) = (8L * var_118)
let (var_128: EnvStack2) = method_3((var_107: uint64), (var_109: System.Collections.Generic.Stack<Env3>), (var_108: uint64), (var_127: int64))
let (var_129: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_121: (Union0 ref)))
let (var_130: (Union0 ref)) = var_128.mem_0
let (var_131: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_130: (Union0 ref)))
// Cuda join point
// method_7((var_129: ManagedCuda.BasicTypes.CUdeviceptr), (var_131: ManagedCuda.BasicTypes.CUdeviceptr))
let (var_132: (System.Object [])) = Array.zeroCreate<System.Object> (System.Convert.ToInt32(2L))
var_132.[int32 0L] <- (var_131 :> System.Object)
var_132.[int32 1L] <- (var_129 :> System.Object)
let (var_133: ManagedCuda.CudaKernel) = ManagedCuda.CudaKernel("method_7", var_83, var_1)
let (var_134: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(32u, 1u, 1u)
var_133.set_GridDimensions(var_134)
let (var_135: ManagedCuda.VectorTypes.dim3) = ManagedCuda.VectorTypes.dim3(128u, 1u, 1u)
var_133.set_BlockDimensions(var_135)
let (var_136: float32) = var_133.Run(var_132)
let (var_137: ManagedCuda.BasicTypes.CUdeviceptr) = method_1((var_130: (Union0 ref)))
let (var_138: (int64 [])) = Array.zeroCreate<int64> (System.Convert.ToInt32(8L))
var_110.CopyToHost(var_138, var_137)
var_110.Synchronize()
let (var_139: string) = method_9((var_138: (int64 [])))
System.Console.WriteLine(var_139)
var_130 := Union0Case1
var_121 := Union0Case1

