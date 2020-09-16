﻿module Spiral.Prepass

type Id = int32
//type FreeVars = {|ty : int; term : int|}
type FreeVars = unit
//type Range = { uri : string; range : Config.VSCRange }
type Range = BlockParsing.Range

type Macro =
    | MText of string
    | MType of T
    | MTerm of E
and TypeMacro =
    | TMText of string
    | TMType of T
and RecordWith =
    | RSymbol of (Range * string) * E
    | RSymbolModify of (Range * string) * E
    | RVar of (Range * E) * E
    | RVarModify of (Range * E) * E
and RecordWithout =
    | WSymbol of Range * string
    | WVar of Range * E
and PatRecordMember =
    | Symbol of (Range * string) * Id
    | Var of (Range * Id) * Id
and E =
    | EB of Range
    | EV of Id
    | ELit of Range * Tokenize.Literal
    | EDefaultLit of Range * string * T
    | ESymbolCreate of Range * string
    | EType of Range * T
    | EApply of Range * E * E
    | EFun of Range * FreeVars * Id * E * T
    | ERecursive of E ref
    | EForall of Range * FreeVars * Id * E
    | ERecBlock of Range * (Id * E) list * on_succ: E
    | ERecordWith of Range * E list * RecordWith list * RecordWithout list
    | ERecord of Map<string, E> // Used for modules.
    | EOp of Range * BlockParsing.Op * E list
    | EJoinPoint of Range * E
    | EAnnot of Range * E * T
    | ETypecase of Range * T * (T * E) list
    | EModuleOpen of Range * (Range * Id) * (Range * string) list * on_succ: E
    | EIfThenElse of Range * E * E * E
    | EIfThen of Range * E * E
    | EPairCreate of Range * E * E
    | ESeq of Range * E * E
    | EHeapMutableSet of Range * E * E
    | EReal of Range * E
    | EMacro of Range * Macro list
    | EInline of Range * FreeVars * E
    | EPrototypeApply of Id * T
    // Regular pattern matching
    | ELet of Range * Id * E * E
    | EPairTest of Range * bind: Id * pat1: Id * pat2: Id * on_succ: E * on_fail: E
    | EKeywordTest of Range * string * bind: Id * on_succ: E * on_fail: E
    | ERecordTest of Range * PatRecordMember list * bind: Id * on_succ: E * on_fail: E
    | EAnnotTest of Range * T * bind: Id * on_succ: E * on_fail: E
    | ELitTest of Range * Tokenize.Literal * bind: Id * on_succ: E * on_fail: E
    | EUnitTest of Range * bind: Id * on_succ: E * on_fail: E
    | EHigherOrderTest of Range * ho: Id * bind: Id * on_succ: E * on_fail: E
    // Typecase
    | ETypeLet of Range * Id * E * E
    | ETypePairTest of Range * bind: Id * pat1: Id * pat2: Id * on_succ: E * on_fail: E
    | ETypeFunTest of Range * bind: Id * pat1: Id * pat2: Id * on_succ: E * on_fail: E
    | ETypeKeywordTest of Range * string * bind: Id * on_succ: E * on_fail: E
    | ETypeRecordTest of Range * Map<string,Id> * bind: Id * on_succ: E * on_fail: E
    | ETypeUnitTest of Range * bind: Id * on_succ: E * on_fail: E
    | ETypeHigherOrderTest of Range * ho: Id * bind: Id * on_succ: E * on_fail: E
    | ETypeHigherOrderDestruct of Range * pat: Id list * bind: Id * on_succ: E * on_fail: E
and T =
    | TUnit of Range
    | TV of Id
    | TPair of Range * T * T
    | TFun of Range * T * T
    | TRecord of Range * Map<string,T>
    | TUnion of Range * Map<string,T>
    | TSymbol of Range * string
    | TApply of Range * T * T
    | TPrim of Range * Config.PrimitiveType
    | TTerm of Range * E
    | TMacro of Range * TypeMacro list
    | TArrow of Range * FreeVars * Id * T
    | TNominal of Id

open FSharpx.Collections

open BlockParsing
open TypecheckingUtils
type TopEnv = {
    prototypes : Map<int,E> PersistentVector
    nominals : {|body : T; name : string|} PersistentVector
    term : Map<string,E>
    ty : Map<string,T>
    }

type Env = {
    term : {| env : Map<string,E>; i : Id; i_rec : Id |}
    ty : {| env : Map<string,T>; i : Id |}
    }

let add_term (e : Env) k v =
    let term = e.term
    {e with term = {|term with i = term.i+1; env = Map.add k v term.env|} }

let add_term_rec (e : Env) k v =
    let term = e.term
    {e with term = {|term with i_rec = term.i_rec-1; env = Map.add k v term.env|} }

let add_ty (e : Env) k v =
    let ty = e.ty
    {e with ty = {|ty with i = ty.i+1; env = Map.add k v ty.env|} }

let add_term_var (e : Env) k = e.term.i, add_term e k (EV e.term.i)
let add_term_rec_var (e : Env) k = e.term.i_rec, add_term e k (EV e.term.i_rec)
let add_ty_var (e : Env) k = e.ty.i, add_ty e k (TV e.ty.i)

type PrepassError =
    | RecordIndexFailed of string

exception PrepassException of (Range * PrepassError) list

let compile_pattern _ = failwith "TODO"
let compile_typecase _ = failwith "TODO"

let prepass (top_env : TopEnv) expr =
    let v_term (env : Env) x = Map.tryFind x env.term.env |> Option.defaultWith (fun () -> top_env.term.[x])
    let v_ty (env : Env) x =  Map.tryFind x env.ty.env |> Option.defaultWith (fun () -> top_env.ty.[x])
    let rec ty (env : Env) x =
        let f = ty env
        match x with
        | RawTWildcard _ -> failwith "Compiler error: Annotation with wildcards should have been stripped."
        | RawTMetaVar _ -> failwith "Compiler error: This should have been compiled away in typecase."
        | RawTForall _ -> failwith "Compiler error: Foralls are not allowed at the type level."
        | RawTB r -> TUnit r
        | RawTVar(r,a) -> v_ty env a
        | RawTPair(r,a,b) -> TPair(r,f a,f b)
        | RawTFun(r,a,b) -> TFun(r,f a,f b)
        | RawTRecord(r,l) -> TRecord(r,Map.map (fun _ -> f) l)
        | RawTUnion(r,l) -> TUnion(r,Map.map (fun _ -> f) l)
        | RawTSymbol(r,a) -> TSymbol(r,a)
        | RawTApply(r,a,b) ->
            match f a, f b with
            | TRecord(_,a), TSymbol(_,b) ->
                match Map.tryFind b a with
                | Some x -> x
                | None -> raise (PrepassException [r,RecordIndexFailed b])
            | a,b -> TApply(r,a,b)
        | RawTPrim(r,a) -> TPrim(r,a)
        | RawTTerm(r,a) -> TTerm(r,term false env a)
        | RawTMacro(r,l) -> 
            let f = function 
                | RawMacroText(r,a) -> TMText a
                | RawMacroTypeVar(r,a) -> TMType(v_ty env a)
                | RawMacroTermVar _ -> failwith "Compiler error: Term vars should not appear on the type level."
            TMacro(r,List.map f l)
    and term is_top_down env x =
        let f = term is_top_down env
        match x with
        | RawB r -> EB r
        | RawV(r,a) -> v_term env a
        | RawBigV(r,a) -> EApply(r,v_term env a,EB r)
        | RawLit(r,a) -> ELit(r,a)
        | RawDefaultLit(r,a) -> failwith "TODO"
        | RawSymbolCreate(r,a) -> ESymbolCreate(r,a)
        | RawType(r,a) -> EType(r,ty env a)
        | RawMatch(r,a,b) -> compile_pattern is_top_down (Some a) b
        | RawFun(r,a) -> compile_pattern is_top_down None a
        | RawTypecase(r,a,b) -> compile_typecase a b
        | RawForall(r,((_,(name,_)),_),b) -> 
            let id, env = add_ty_var env name
            EForall(r,(),id,term is_top_down env b)
        | RawRecBlock(r,l,on_succ) ->
            let l,env = List.mapFold (fun env ((r,name),body) -> let id,env = add_term_rec_var env name in (id,body), env) env l
            ERecBlock(r,List.map (fun (id,body) -> id, term is_top_down env body) l,term is_top_down env on_succ)
        | RawRecordWith(r,a,b,c) ->
            let a = List.map f a
            let b = b |> List.map (function
                | RawRecordWithSymbol((r,a),b) -> RSymbol((r,a),f b)
                | RawRecordWithSymbolModify((r,a),b) -> RSymbolModify((r,a),f b)
                | RawRecordWithInjectVar((r,a),b) -> RVar((r,v_term env a),f b)
                | RawRecordWithInjectVarModify((r,a),b) -> RVarModify((r,v_term env a),f b))
            let c = c |> List.map (function
                | RawRecordWithoutSymbol(r,a) -> WSymbol(r,a)
                | RawRecordWithoutInjectVar(r,a) -> WVar(r,v_term env a))
            ERecordWith(r,a,b,c)
        | RawOp(r,a,b) -> EOp(r,a,List.map f b)
        | RawJoinPoint(r,a) -> EJoinPoint(r,f a)
        | RawAnnot(r,a,b) -> EAnnot(r,f a,ty env b)
        | RawModuleOpen (r,a,l,on_succ) ->
            let a,b = 
                match top_env.term.[snd a], top_env.ty.[snd a] with
                | ERecord a, TRecord(_, b) ->
                    List.fold (fun (a,b) (_,x) ->
                        match Map.find x a, Map.find x b with
                        | ERecord a, TRecord(_, b) -> a,b
                        | _ -> failwith "Compiler error: Module open's symbol index should have been validated."
                        ) (a,b) l
                | _ -> failwith "Compiler error: Module open should have been validated."
            let env =
                let combine e m = Map.foldBack Map.add m e
                {
                term = {|env.term with env = combine env.term.env a|}
                ty = {|env.ty with env = combine env.ty.env b|}
                }
            term is_top_down env on_succ
        | RawApply(r,a,b) ->
            match f a, f b with
            | ERecord a, ESymbolCreate(_,b) ->
                match Map.tryFind b a with
                | Some x -> x
                | None -> raise (PrepassException [r,RecordIndexFailed b])
            | a,b -> EApply(r,a,b)
        | RawIfThenElse(r,a,b,c) -> EIfThenElse(r,f a,f b,f c)
        | RawIfThen(r,a,b) -> EIfThen(r,f a,f b)
        | RawPairCreate(r,a,b) -> EPairCreate(r,f a,f b)
        | RawSeq(r,a,b) -> ESeq(r,f a,f b)
        | RawHeapMutableSet(r,a,b) -> EHeapMutableSet(r,f a,f b)
        | RawReal(r,a) -> EReal(r,term false env a)
        | RawMacro(r,a) -> // TODO: Don't forget to fill in the types for the macros.
            let a = a |> List.map (function
                | RawMacroText(r,a) -> MText a
                | RawMacroTermVar(r,a) -> MTerm(v_term env a)
                | RawMacroTypeVar(r,a) -> MType(v_ty env a)
                )
            EMacro(r,a)
        | RawMissingBody _ -> failwith "Compiler error: The missing body cases should have been validated."
        | RawInline(r,a) -> EInline(r,(),f a)

    let env =
        {
        term = {|env=Map.empty; i=0; i_rec= -1|}
        ty = {|env=Map.empty; i=0|}
        }

    let eval_type ((r,(name,kind)) : HoVar) on_succ env =
        let id, env = add_ty_var env name
        TArrow(r,(),id,on_succ env)
    let eval_type' env l body = List.foldBack eval_type l (fun env -> ty env body) env
    match expr with
    | BundleType(r,(_,name),l,body) -> {top_env with ty = Map.add name (eval_type' env l body) top_env.ty}
    | BundleNominal l ->
        let env,_ = List.fold (fun (env,i) (r,(_,name),l,body) -> add_ty env name (TNominal i), i+1) (env, top_env.nominals.Length) l
        let ty,nominals = 
            List.fold (fun (ty, nominals) (r,(_,name),l,body) -> 
                let x = eval_type' env l body
                Map.add name x ty, PersistentVector.conj {|body=x; name=name|} nominals
                ) (top_env.ty, top_env.nominals) l
        {top_env with ty = ty; nominals = nominals}
    | BundleInl(_,(_,name),body,is_top_down) ->
        {top_env with term = Map.add name (term is_top_down env body) top_env.term}
    | BundleRecInl l ->
        let l, env = 
            List.mapFold (fun env (_,(_,name),_,_ as x) -> 
                let r = ref Unchecked.defaultof<_>
                (x,r), add_term_rec env name (ERecursive r)
                ) env l
        let term = 
            List.fold (fun top_env_term ((_,(_,name),body,is_top_down),r) ->
                r := term is_top_down env body
                Map.add name !r top_env_term
                ) top_env.term l
        {top_env with term = term}
    | BundlePrototype(r,(_,name),_,_,_) ->
        let x = EForall(r,(),0,EPrototypeApply(top_env.prototypes.Length,TV 0))
        {top_env with term = Map.add name x top_env.term; prototypes = PersistentVector.conj Map.empty top_env.prototypes}
    | BundleInstance(r,(_,prot),(_,ins),l,body) ->
        let prot_id = failwith "TODO"
        let ins_id = match top_env.ty.[ins] with TNominal i -> i | _ -> failwith "Compiler error: Expected a nominal id."
        // TODO: Don't forget to fill in the implicit foralls as well.
        let body = List.foldBack (fun x s -> RawForall(range_of_typevar x +. range_of_expr s,x,s)) l body |> term true env 
        {top_env with prototypes = PersistentVector.update prot_id (Map.add ins_id body top_env.prototypes.[prot_id]) top_env.prototypes}
        