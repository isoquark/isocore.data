namespace IQ.Core.Math

/// <summary>
/// Provides basic mechanism for yielding an ordered numeric sequence with specified
/// characteristics
/// </summary>
module ArithmeticEnumerator =
    /// <summary>
    /// Creates an enumerator that whose types are statically resolved
    /// </summary>
    /// <param name="initial">The first value emitted</param>
    /// <param name="min">The minimum value than can potentially be emitted</param>
    /// <param name="inc">The distance between yielded values</param>
    /// <param name="max">The maximum value that can potentially be emitted</param>
    /// <param name="cycle">Whether the sequence cycles back to the minimum value when the maximum is reached</param>
    let inline createInline (initial : ^T) (min : ^T) (inc : ^S) (max : ^T) cycle = 
        let s = seq{ 
               let mutable cur =  initial
               while (cur < max) do
                    yield cur
                    cur <- cur + inc  
                    if cur = max then
                        yield cur
                        if cycle then
                            cur <- min                
           }
        s.GetEnumerator()

    /// <summary>
    /// Creates an enumerator that whose types are generic
    /// </summary>
    /// <param name="initial">The first value emitted</param>
    /// <param name="min">The minimum value than can potentially be emitted</param>
    /// <param name="inc">The distance between yielded values</param>
    /// <param name="max">The maximum value that can potentially be emitted</param>
    /// <param name="cycle">Whether the sequence cycles back to the minimum value when the maximum is reached</param>
    let createGeneric (initial : 'T) (min : 'T) (inc : 'T) (max : 'T) cycle =
        let calc = Calculator.specific<'T>()
        let s = seq{ 
               let mutable cur =  initial
               while (calc.LessThan(cur, max)) do
                    yield cur
                    cur <- calc.Add(cur, inc)
                    if calc.Equal(cur, max) then
                        yield cur
                        if cycle then
                            cur <- min                
           }
        s.GetEnumerator()


