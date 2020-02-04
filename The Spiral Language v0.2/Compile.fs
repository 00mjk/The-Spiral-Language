﻿module Spiral.Compile

open System.Collections.Generic
open System.Diagnostics
open System
open Spiral.Utils
open Spiral.ParserCombinators
open Spiral.Parsing
open Spiral.Prepass
open Spiral.PartEval
open Spiral.CodegenFsharp

type Timings =
    {
    parse : TimeSpan
    prepass : TimeSpan
    peval : TimeSpan
    codegen : TimeSpan
    }

    member x.Add(y) =
        {
        parse = x.parse + y.parse
        prepass = x.prepass + y.prepass
        peval = x.peval + y.peval
        codegen = x.codegen + y.codegen
        }

type Watches = 
    {
    parse : Stopwatch
    prepass : Stopwatch
    peval : Stopwatch
    codegen : Stopwatch
    }

    member x.Elapsed: Timings =
        {
        parse = x.parse.Elapsed
        prepass = x.prepass.Elapsed
        peval = x.peval.Elapsed
        codegen = x.codegen.Elapsed
        }

let inline timeit (d: Stopwatch) f =
    d.Start()
    let x = f ()
    d.Stop()
    x

type CompilationEnv = {
    keywords : KeywordEnv
    types : Map<string,TExpr>
    values : Map<string,Expr>
    }

exception CompilationError of string

let module' (timings : Watches) (d : CompilationEnv) (x : SpiralModule) =
    let p = timeit timings.parse <| fun _ -> parse x
    match p with
    | Ok(var_positions,expr) ->
        let p = timeit timings.prepass <| fun _ -> prepass var_positions d.keywords d.types d.values expr
        match p with
        | Ok(t,v) -> 
            let v = Module(Seq.fold (fun m (k,v) -> Map.add (d.keywords.To k) v m) Map.empty v)
            let t = TModule(Seq.fold (fun m (k,v) -> Map.add (d.keywords.To k) v m) Map.empty t)
            {d with types=Map.add x.name t d.types; values=Map.add x.name v d.values}
        | Error er -> raise (CompilationError er)
    | Error er -> raise (CompilationError er)
        
let modules timings (d : CompilationEnv) (x : SpiralModule) =
    let m = Dictionary(HashIdentity.Reference)
    let rec f (d : CompilationEnv) (x : SpiralModule) =
        memoize m (fun _ -> module' timings (List.fold f d x.prerequisites) x) x

    f d x

type SpiralCompilerSettings = {
    trace_length : int // The length of the error messages.
    filter_list : string list // List of modules to be ignored in the trace.
    }

let show_trace (settings: SpiralCompilerSettings) (trace: Trace) message = 
    let m = Dictionary(HashIdentity.Reference)
    let filter_set = HashSet(settings.filter_list,HashIdentity.Structural)
    let error = System.Text.StringBuilder(1024)
    let x =
        List.toArray trace
        |> Array.filter (fun {module_={name=x}} -> filter_set.Contains x = false)
    if x.Length > 0 then
        x.[0..(min x.Length settings.trace_length - 1 |> max 0)]
        |> Array.rev
        |> Array.iter (show_position m error)
    error.AppendLine message |> ignore
    error.ToString()

let compile (settings: SpiralCompilerSettings) x =
    let timings = 
        {
        parse = Stopwatch()
        prepass = Stopwatch()
        peval = Stopwatch()
        codegen = Stopwatch()
        }

    try
        let env : CompilationEnv = {
            keywords = KeywordEnv()
            types = Map.empty
            values = Map.empty
            }
        let env = modules timings env x
        match env.values.[x.name] with
        | Module l -> 
            match Map.tryFind (env.keywords.To "main") l with
            | Some (Inl(a,b)) -> 
                let dex : ExternalLangEnv = {
                    keywords = env.keywords
                    hc_table = HashConsing.HashConsTable()
                    join_point_method = Dictionary(HashIdentity.Reference)
                    memoized_modules_type = Dictionary(HashIdentity.Reference)
                    memoized_modules_value = Dictionary(HashIdentity.Reference)
                    }

                let seq = ResizeArray()
                let d : LangEnv = {
                    trace = []
                    seq = seq
                    cse = [Dictionary(HashIdentity.Structural)]
                    i = ref 0
                    env_global_type = [||]
                    env_global_value = [||]
                    env_stack_type = Array.zeroCreate b.type'.stack_size
                    env_stack_type_ptr = 0
                    env_stack_value = Array.zeroCreate b.value.stack_size
                    env_stack_value_ptr = 0
                    }
            
                let _ : Data = timeit timings.peval <| fun _ -> partial_eval_value dex d a
                timeit timings.codegen <| fun _ -> codegen dex (seq.ToArray())
                |> Ok
            | Some(Forall _) -> raise <| CompilationError "main has to be a regular function, not a forall."
            | Some _ -> raise <| CompilationError "main has to be a function."
            | None -> raise <| CompilationError "main has to be present in the last module."
        | _ -> failwith "Compiler error: Module that has been compiled should always be in the environment."
    with 
        | :? CompilationError as x -> Error x.Data0
        | :? TypeError as x -> Error(show_trace settings x.Data0 x.Data1)
        | :? CodegenError as x -> Error(x.Data0)
        | :? CodegenErrorWithPos as x -> Error(show_trace {settings with filter_list=[]} x.Data0 x.Data1)
    |> fun x -> timings.Elapsed, x