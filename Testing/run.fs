#if INTERACTIVE
#r @"..\packages\FParsec.1.0.3\lib\net40-client\FParsecCs.dll"
#r @"..\packages\FParsec.1.0.3\lib\net40-client\FParsec.dll"
#r @"..\packages\ManagedCuda-CUBLAS.8.0.22\lib\net46\CudaBlas.dll"
#r @"..\packages\ManagedCuda-CURAND.8.0.22\lib\net46\CudaRand.dll"
#r @"Microsoft.CSharp"
#r @"..\The Spiral Language\bin\Release\The_Spiral_Language.dll"
#endif

open Spiral.Lib
open Spiral.Tests
open System.IO
open System.Diagnostics
open Spiral.Types

let cfg = Spiral.Types.cfg_testing

let test1: SpiralModule =
    {
    name="test1"
    prerequisites=[]
    opens=[]
    description="Does it run?"
    code=
    """
inl a = 5
inl b = 10
dyn (a + b)
    """
    }

let test2: SpiralModule =
    {
    name="test2"
    prerequisites=[]
    opens=[]
    description="Do the join points work?"
    code=
    """
inl a () = join 5
inl b () = join 10
a () + b ()
    """
    }

let test3: SpiralModule =
    {
    name="test3"
    prerequisites=[]
    opens=[]
    description="Does `dyn` work?"
    code=
    """
inl a = dyn 5
inl b = dyn 10
a + b
    """
    }

let test4: SpiralModule =
    {
    name="test4"
    prerequisites=[]
    opens=[]
    description="Does the and pattern work correctly?"
    code=
    """
inl f (a, b) (c, d) = dyn (a+c,b+d)
inl q & (a, b) = dyn (1,2)
inl w & (c, d) = dyn (3,4)
f q w
    """
    }

let test5: SpiralModule =
    {
    name="test5"
    prerequisites=[]
    opens=[]
    description="Does basic pattern matching work?"
    code=
    """
inl f = function
    | .Add x y -> join (x + y)
    | .Sub x y -> join (x - y)
    | .Mult x y -> join (x * y)
inl a = f .Add 1 2
inl b = f .Sub 1 2
inl c = f .Mult 1 2
a, b, c
    """
    }

let test6: SpiralModule =
    {
    name="test6"
    prerequisites=[]
    opens=[]
    description="Does returning type level methods from methods work?"
    code=
    """
inl min n = join
    inl tes a = join
        inl b -> join
            inl c -> join
                inl d -> join a,b,c
    tes 1 2 (2.2,3,4.5)
min 10
    """
    }

let test7: SpiralModule =
    {
    name="test7"
    prerequisites=[]
    opens=[]
    description="Do active patterns work?"
    code=
    """
inl f op1 op2 op3 = function
    | !op1 (.Some, x) -> x
    | !op2 (.Some, x) -> x
    | !op3 (.Some, x) -> x

inl add = function
    | .Add -> .Some, inl x y -> x + y
    | _ -> .None
inl sub = function
    | .Sub -> .Some, inl x y -> x - y
    | _ -> .None
inl mult = function
    | .Mult -> .Some, inl x y -> x * y
    | _ -> .None

inl f = f add sub mult

inl a = f .Add 1 2
inl b = f .Sub 1 2
inl c = f .Mult 1 2
dyn (a, b, c)
    """
    }

let test8: SpiralModule =
    {
    name="test8"
    prerequisites=[]
    opens=[]
    description="Does the basic union type work?"
    code=
    """
inl option_int = .Some, (1,2,3) \/ .None

inl x = join Type (box: .None to: option_int)
match x with
| #(.Some, x) -> x 
| #(.None) -> 0,0,0
    """
    }

let test9: SpiralModule =
    {
    name="test9"
    prerequisites=[]
    opens=[]
    description="Does the partial evaluator optimize unused match cases?"
    code=
    """
inl ab = .A \/ .B
inl ab x = Type (box: x to: ab)
inl #a,#b,#c = join (ab .A, ab .A, ab .A)
match a,b,c with
| .A, _, _ -> 1
| _, .A, _ -> 2
| _, _, .A -> 3
| _ -> 4
    """
    }

let test10: SpiralModule =
    {
    name="test10"
    prerequisites=[]
    opens=[]
    description="Do the join points get filtered?"
    code=
    """
inl ab x = Type (box: x to: (.A \/ .B))
match join (ab .A, ab .A, ab .A, ab .A) with
| #(.A), #(.A), _, _ -> join 1
| _, _, #(.A), #(.A) -> join 2
| #(.A), #(.B), #(.A), #(.B) -> join 3
| _ -> join 4
    """
    }

let test11: SpiralModule =
    {
    name="test11"
    prerequisites=[]
    opens=[]
    description="Do the nested patterns work?"
    code=
    """
inl a = type (1,2)
inl b = type (1,a,a)
inl a,b = (inl x -> Type (box: x to: a)), (inl x -> Type (box: x to: b))
inl x = b (1, a (2,3), a (4,5))
match x with
| _, (x, _), (_, y) -> x + y
| _, _, _ -> 0
| _ :: () -> 0
|> dyn
    """
    }

let test12: SpiralModule =
    {
    name="test12"
    prerequisites=[]
    opens=[]
    description="Does recursive pattern matching work on static data?"
    code=
    """
inl rec p = function
    | .Some, x -> p x
    | .None -> 0
p (.Some, .None)
|> dyn
    """
    }

let test13: SpiralModule =
    {
    name="test13"
    prerequisites=[]
    opens=[]
    description="A more complex interpreter example on static data."
    code=
    """
inl rec expr x = 
    Type (
        name: "Arith"
        join:inl _ ->
            v: x
            \/ add: expr x, expr x
            \/ mult: expr x, expr x
        )
inl int_expr x = Type (box: x to: expr 0)
inl v x = int_expr (v: x)
inl add a b = int_expr (add: a, b)
inl mult a b = int_expr (mult: a, b)
inl a = add (v 1) (v 2)
inl b = add (v 3) (v 4)
inl c = mult a b

inl rec interpreter_static #x = 
    match x with
    | v: x -> x
    | add: a, b -> interpreter_static a + interpreter_static b
    | mult: a, b -> interpreter_static a * interpreter_static b
interpreter_static c
|> dyn
    """
    }

let test14: SpiralModule =
    {
    name="test14"
    prerequisites=[]
    opens=[]
    description="Does recursive pattern matching work on partially static data?"
    code=
    """
inl rec expr x = 
    Type (
        name: "Arith"
        join:inl _ ->
            v: x
            \/ add: expr x, expr x
            \/ mult: expr x, expr x
        )
inl int_expr x = Type (box: x to: expr 0)
inl v x = int_expr (v: x)
inl add a b = int_expr (add: a, b)
inl mult a b = int_expr (mult: a, b)
inl a = add (v 1) (v 2)
inl b = add (v 3) (v 4)
inl c = dyn (mult a b)

inl rec inter x = join
    inl #x = x
    match x with
    | v: x -> x
    | add: a, b -> inter a + inter b
    | mult: a, b -> inter a * inter b
    : 0

inter c
    """
    }

let test15: SpiralModule =
    {
    name="test15"
    prerequisites=[]
    opens=[]
    description="Does the object with unary patterns?"
    code=
    """
inl f = 
    [
    add = inl a b -> a + b
    mult = inl a b -> a * b
    ]
    
f .add 1 2 |> dyn
    """
    }

let test16: SpiralModule =
    {
    name="test16"
    prerequisites=[]
    opens=[]
    description="Do var union types work?"
    code=
    """
inl t = 0 \/ 0.0
if dyn true then Type (box: 0 to: t)
else Type (box: 0.0 to: t)
    """
    }

let test17: SpiralModule =
    {
    name="test17"
    prerequisites=[]
    opens=[]
    description="Do modules work?"
    code=
    """
inl m =
    inl x = 2
    inl y = 3.4
    inl z = "123"
    {x y z}
dyn (m.x, m.y, m.z)
    """
    }

let test18: SpiralModule =
    {
    name="test18"
    prerequisites=[]
    opens=[]
    description="Does the term casting of functions work?"
    code=
    """
inl add a b (c, (d, e), f) = a + b + c + d + e + f
inl f = Type (term_cast: add 8 (dyn 7) with: type (0,(0,0),0))
f (1,(2,5),3)
    """
    }

let test19: SpiralModule =
    {
    name="test19"
    prerequisites=[]
    opens=[]
    description="Does pattern matching on union non-tuple types work? Do type annotation patterns work?"
    code=
    """
inl t = 0 \/ 0.0
inl #x = Type (box: 3.5 to: t)
match x with
| x : 0 -> x * x
| x : 0.0 -> x + x
|> dyn
    """
    }

let test20: SpiralModule =
    {
    name="test20"
    prerequisites=[]
    opens=[]
    description="Does defining user operators work?"
    code=
    """
inl (.+) a b = a + b
inl x = 2 * 22 .+ 33

inl f op a b = op a b
f (*) 2 x
|> dyn
    """
    }

let test21: SpiralModule =
    {
    name="test21"
    prerequisites=[]
    opens=[]
    description="Do when and as patterns work?"
    code=
    """
inl f = function
    | a,b,c as q when a < 10 -> q
    | _ -> 0,0,0
dyn (f (1,2,3))
    """
    }

let test22: SpiralModule =
    {
    name="test22"
    prerequisites=[]
    opens=[]
    description="Do literal pattern matchers work? Does partial evaluation of equality work?"
    code=
    """
inl f x = 
    match x with
    | 0 -> "0", x
    | 1 -> "1", x
    | false -> "false", x
    | true -> "true", x
    | "asd" -> "asd", x
    | 1i8 -> "1i8", x
    | 5.5 -> "5.5", x
    | _ -> "unknown", x

dyn (f 0, f 1, f false, f true, f "asd", f 1i8, f 5.5, f 5f64)
    """
    }

let test23: SpiralModule =
    {
    name="test23"
    prerequisites=[]
    opens=[]
    description="Does the tuple cons pattern work?"
    code=
    """
inl f = function x1 :: x2 :: x3 :: xs -> 3 | x1 :: x2 :: xs -> 2 | x1 :: xs -> 1 | () -> 0

dyn (f (), f (1 :: ()), f (1,2))
    """
    }

let test24: SpiralModule =
    {
    name="test24"
    prerequisites=[]
    opens=[]
    description="Does pattern matching work redux?"
    code=
    """
inl t = 0, 0 \/ 0

inl #x = Type (box: 1,1 to: t) |> dyn
match x with
| a,b -> 0
| c -> c
    """
    }

let test25: SpiralModule =
    {
    name="test25"
    prerequisites=[]
    opens=[]
    description="Do recursive algebraic datatypes work?"
    code=
    """
inl rec List x = 
    Type (
        name: "List"
        join: inl _ -> .nil \/ cons: x, List x
        )

inl t x = Type (box: x to: List 0)
inl nil = t .nil
inl cons x xs = t (cons: x, xs)

inl rec sum (!dyn s) l = join
    inl #l = l
    match l with
    | cons: x, xs -> sum (s + x) xs
    | .nil -> s
    : 0

nil |> cons 3 |> cons 2 |> cons 1 |> dyn |> sum 0
        """
    }

let test26: SpiralModule =
    {
    name="test26"
    prerequisites=[]
    opens=[]
    description="Does passing types into types work?"
    code=
    """
inl a = .A, (0, 0) \/ .B, ""
inl b = a \/ .Hello
inl box a #b = Type (box: b to: a)
(.A, (2,3)) |> box a |> dyn |> box b
    """
    }

let test27: SpiralModule =
    {
    name="test27"
    prerequisites=[]
    opens=[]
    description="Do the module map and fold functions work?"
    code=
    """
inl m = {a=1;b=2;c=3}
inl m' = Record (map:inl (key:value:) -> value * 2) m
dyn (m', Record (foldl: inl (key:state:value:) -> state + value) 0 m')
    """
    }

let test28: SpiralModule =
    {
    name="test28"
    prerequisites=[]
    opens=[]
    description="Does a simple stackified function work?"
    code=
    """
inl a = dyn 1
inl b = dyn 2
inl add c d = a + b + c + d
inl f g c d = join g c d
f (stack add) (dyn 3) (dyn 4)
    """
    }

let test29: SpiralModule =
    {
    name="test29"
    prerequisites=[]
    opens=[]
    description="Does case on union types with recursive types work properly?"
    code=
    """
inl rec List x = 
    Type (
        name: "List"
        join: inl _ -> .nil \/ cons: x, List x
        )

inl Res =
    0
    \/ 0, 0
    \/ List 0

inl #x = Type (box: 1 to:Res) |> dyn
match x with
| _ : 0 -> 1
| (a, b) -> 2
| _ : (List 0) -> 3
    """
    }

let test30: SpiralModule =
    {
    name="test30"
    prerequisites=[]
    opens=[]
    description="Does a simple heapified function work?"
    code=
    """
inl a = dyn 1
inl b = dyn 2
inl add c d = a + b + c + d
inl f g c d = join g c d
f (heap add) (dyn 3) (dyn 4)
    """
    }

let test31: SpiralModule =
    {
    name="test31"
    prerequisites=[]
    opens=[]
    description="Does a simple heapified module work?"
    code=
    """
inl m = heap { a=dyn 1; b=dyn 2 }
inl add c d = 
    inl {a b} = indiv m
    a + b + c + d
inl f g c d = join g c d
f (heap add) (dyn 3) (dyn 4)
    """
    }

let test32: SpiralModule =
    {
    name="test32"
    prerequisites=[]
    opens=[]
    description="Is type constructor of an int64 an int64?"
    code=
    """
Type (box: dyn 1 to: 0)
    """
    }

let test33: SpiralModule =
    {
    name="test33"
    prerequisites=[]
    opens=[]
    description="Does the mutable layout type get unpacked multiple times?"
    code=
    """
inl box a b = Type (box: b to: a)
inl q = heapm <| dyn {a=1;b=2;c=3} \/ heapm <| dyn {a=1;b=2} \/ heap <| dyn (1,2,3)
inl #x = {a=1;b=2;c=3} |> dyn |> heapm |> box q |> dyn
match indiv x with
| {a} as x ->
    inl {b} = x
    match x with
    | {c} -> a+b+c
    | _ -> a+b
| a,b,c -> a*b*c
    """
    }

let test34: SpiralModule =
    {
    name="test34"
    prerequisites=[]
    opens=[]
    description="Does this compile into just one method? Are the arguments reversed in the method call?"
    code=
    """
inl rec f a b = join
    if dyn true then f b a
    else a + b
    : 0
f (dyn 1) (dyn 2)
    """
    }

let test35: SpiralModule =
    {
    name="test35"
    prerequisites=[]
    opens=[]
    description="Does result in a `type ()`?"
    code=
    """
inl ty = .Up \/ .Down \/ heap <| dyn {q=1;block=(1,(),3)}
inl r =
    inl #x = dyn (Type (box: .Up to: ty))
    match x with
    | .Up -> {q=1;block=(1,(),3)}
    | .Down -> {q=2;block=(2,(),4)}
    | _ -> {q=1;block=(1,(),3)}
Type (box: heap r to: ty)
    """
    }

let test36: SpiralModule =
    {
    name="test36"
    prerequisites=[]
    opens=[]
    description="Does the module pattern work?"
    code=
    """
inl f {a b c} = a + b + c
inl x =
    {
    a=1
    b=2
    c=3
    }

dyn (f {x with a = 4})
    """
    }

let test37: SpiralModule =
    {
    name="test37"
    prerequisites=[]
    opens=[]
    description="Does the nested module pattern work?"
    code=
    """
inl f {name p={x y}} = name,(x,y)
inl x = { name = "Coord" }

f {x with 
    p = { x = 1
          y = 2 }}
|> dyn
    """
    }

let test38: SpiralModule =
    {
    name="test38"
    prerequisites=[]
    opens=[]
    description="Does the nested module pattern with rebinding work?"
    code=
    """
inl f {name p={y=y' x=x'}} = name,(x',y')
inl x = { name = "Coord" }
f {x with 
    p = { x = 1
          y = 2 }}
|> dyn
    """
    }

let test39: SpiralModule =
    {
    name="test39"
    prerequisites=[]
    opens=[]
    description="Does the lens pattern work? Does self work? Does the semicolon get parsed properly?"
    code=
    """
inl x = { a = { b = { c = 3 } } }

inl f {a={b={c q}}} = c,q
f {x.a.b with q = 4; c = this + 3; d = {q = 12; w = 23}}
|> dyn
    """
    }

let test40: SpiralModule =
    {
    name="test40"
    prerequisites=[]
    opens=[]
    description="Does term casting with an unit return get printed properly?"
    code=
    """
inl add a, b = ()
inl k = Type (term_cast: add with: 0,0)
k (1, 2)
    """
    }

let test41: SpiralModule =
    {
    name="test41"
    prerequisites=[]
    opens=[]
    description="Does the new module creation syntax work?"
    code=
    """
inl a = 1
inl b = 2
inl d = 4
dyn {a b c = 3; d; e = 5}
    """
    }

let test42: SpiralModule =
    {
    name="test42"
    prerequisites=[]
    opens=[]
    description="Is the trace being correctly propagated for TyTs?"
    code=
    """
inl a = dyn 1
inl b = dyn 2
inl c = dyn 3
4 + type 0
    """
    }

let test43: SpiralModule =
    {
    name="test43"
    prerequisites=[]
    opens=[]
    description="Does the partial evaluation of if statements work?"
    code=
    """
inl x = dyn false
inl _ = dyn (x && (x || x && (x || x)))
inl _ = dyn ((x && x || x) || (x || true))
inl _ = dyn (if x then false else x)
dyn (if x then false else true)

//let ((var_1 : bool)) = false
//let ((var_2 : bool)) = true
//let ((var_3 : bool)) = false
//let ((var_4 : bool)) = var_1 = false
    """
    }

let test44: SpiralModule =
    {
    name="test44"
    prerequisites=[]
    opens=[]
    description="Do && and || work correctly?"
    code=
    """
inl a,b,c,d,e = dyn (true, false, true, false, true)
a && b || c && d || e
    """
    }

let test45: SpiralModule =
    {
    name="test45"
    prerequisites=[]
    opens=[]
    description="Does the argument get printed on a type error?"
    code=
    """
inl a : 0f64 = 5
()
    """
    }

let test46: SpiralModule =
    {
    name="test46"
    prerequisites=[]
    opens=[]
    description="Does the recent change to error printing work? This one should give an error."
    code=
    """
55 + id
    """
    }

let test47: SpiralModule =
    {
    name="test47"
    prerequisites=[]
    opens=[]
    description="Does structural polymorphic equality work?"
    code=
    """
{a=1;b=dyn 2;c=dyn 3} = {a=1;b=2;c=3}
    """
    }

let test48: SpiralModule =
    {
    name="test48"
    prerequisites=[]
    opens=[]
    description="Does this destructure trigger an error?"
    code=
    """
inl q = true && dyn true
()
    """
    }

let test49: SpiralModule =
    {
    name="test49"
    prerequisites=[]
    opens=[]
    description="Does changing layout type work?"
    code=
    """
{a=1; b=2} |> dyn |> stack |> heap |> indiv
    """
    }

let test50: SpiralModule =
    {
    name="test50"
    prerequisites=[]
    opens=[]
    description="Does the CSE work as expected?"
    code=
    """
inl !dyn a,b = 2,3
(a+b)*(a+b)
    """
    }

let test51: SpiralModule =
    {
    name="test51"
    prerequisites=[]
    opens=[]
    description="Does the string format work as expected?"
    code=
    """
inl l = 2,2.3,"qwe"
inl q = 1,2
String (format: "{0,-5}{1,-5}{2,-5}" args: l) |> dyn |> ignore
String (format: "{0,-5}{1,-5}{2,-5}" args: dyn l) |> ignore
String (format: (dyn "{0} = {1}") args: dyn q) |> ignore
    """
    }

let test52: SpiralModule =
    {
    name="test52"
    prerequisites=[]
    opens=[]
    description="Does the binary . operator apply if it is directly next to an expression?"
    code=
    """
inl f = function
    | .Hello as x -> .Bye

inl g = function
    | .Bye -> "Bye"

dyn (g f.Hello)
    """
    }

let test53: SpiralModule =
    {
    name="test53"
    prerequisites=[]
    opens=[]
    description="Does the unit closure get printed correctly."
    code=
    """
inl rec loop f i =
    inl f, i = Type (term_cast: f with: ()), dyn i
    inl body _ = if i < 10 then loop (inl _ -> f() + 1) (i + 1) else f()
    join (body() : 0)

loop (inl _ -> 0) 0
    """
    }

let test54: SpiralModule =
    {
    name="test54"
    prerequisites=[]
    opens=[]
    description="Does the prepass memoization work? Intended to be looked directly without the Core library."
    code=
    """
inl f x =
    match x with
    | q,w,e,r,t,z,x,c,v,b,m -> 0
    | (((),a,b) | ({q w e r t y z a b}, _, _)) -> 
        inl f a b = !Add(a, b)
        f a b
    | a,b -> !Add(a, b)
!Dynamize (f ({q=(); w=(); e=(); r=(); t=(); y=(); z=(); a=1; b=2},2,3))
    """
    }

let test55: SpiralModule =
    {
    name="test55"
    prerequisites=[]
    opens=[]
    description="Does the injection pattern work?"
    code=
    """
inl m = {
    a = 123
    b = 456
    }
inl f i {$i=x} = x
dyn (f .a m, f .b m)
    """
    }

let test56: SpiralModule =
    {
    name="test56"
    prerequisites=[]
    opens=[]
    description="Does the injection constructor work?"
    code=
    """
inl f i v m = {m with $i=v}
{}
|> f .a 123
|> f .b 456
|> inl {a b} -> a,b
|> dyn
    """
    }

let test57: SpiralModule =
    {
    name="test57"
    prerequisites=[]
    opens=[]
    description="Does the parser give an error on an indented expression after a statement? It should."
    code=
    """
1 |> ignore
    2
    """
    }

let test58: SpiralModule =
    {
    name="test58"
    prerequisites=[]
    opens=[]
    description="Does the newline after a semicolon work correctly?"
    code=
    """
dyn
    {a=1; b=2; 
     c=3}
    """
    }

let test59: SpiralModule =
    {
    name="test59"
    prerequisites=[]
    opens=[]
    description="Does returning from join points work on nested structures?"
    code=
    """
inl q = {q=1;w=2;e=3}
inl w = {a=q;b=q}
inl e = {z=w;x=w}
inl e = join e
inl e = join e
()
    """
    }

let test60: SpiralModule =
    {
    name="test60"
    prerequisites=[]
    opens=[]
    description="Does structural equality work correctly on union types?"
    code=
    """
inl Q = 0,0 \/ 0
inl f x = join Type (box: x to: Q)
inl a, b = f 1, f 1
a = b
    """
    }

let test61: SpiralModule =
    {
    name="test61"
    prerequisites=[]
    opens=[]
    description="Does the () module-with pattern work?"
    code=
    """
inl k = .q
inl m = { $k = { b = 2 }}

{(m).(k) with a = 1}
|> dyn
    """
    }

let test62: SpiralModule =
    {
    name="test62"
    prerequisites=[]
    opens=[]
    description="Do type_catch and type_raise work?"
    code=
    """
type_catch
    dyn "a" |> ignore
    dyn "b" |> ignore
    dyn "c" |> ignore
    Type (raise: stack 3)
|> indiv |> dyn
    """
    }

let test63: SpiralModule =
    {
    name="test63"
    prerequisites=[]
    opens=[]
    description="Do the keyword arguments get parsed correctly."
    code=
    """
inl add left:right: = left + right
add left:1 right: 2
+ add 
    left: 3 
    right: 7
|> dyn
    """
    }

let tests =
    [|
    test1; test2; test3; test4; test5; test6; test7; test8; test9; 
    test10; test11; test12; test13; test14; test15; test16; test17; test18; test19; 
    test20; test21; test22; test23; test24; test25; test26; test27; test28; test29; 
    test30; test31; test32; test33; test34; test35; test36; test37; test38; test39; 
    test40; test41; test42; test43; test44; test45; test46; test47; test48; test49; 
    test50; test51; test52; test53; test54; test55; test56; test57; test58; test59; 
    test60; test61; test62; test63
    |]


rewrite_test_cache tests cfg None //(Some(0,40))
output_test_to_temp cfg (Path.Combine(__SOURCE_DIRECTORY__ , @"..\Temporary\output.fs")) test63
|> printfn "%s"
|> ignore
