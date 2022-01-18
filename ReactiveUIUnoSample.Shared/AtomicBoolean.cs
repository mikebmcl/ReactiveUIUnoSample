using System;
using System.Threading;
using System.Windows.Input;

namespace ReactiveUIUnoSample
{
    /// <summary>
    /// This is a wrapper around <see cref="Interlocked"/> that uses a value type supported by that type in order to provide boolean functionality.
    /// This is a common pattern for using it:
    /// <code>
    /// if (!ab.Set(true)) { try { runMyCode(); } finally { ab.ForceToFalse(); } }
    /// </code>
    /// The use of try-finally (or try-catch-finally) is critical when using the above pattern. Without it, it is possible that the value of <c>ab</c>
    /// would remain true forever if some exception happened that would result anything after <c>runMyCode();</c> never running. The finally block ensures
    /// that <c>ab</c> is returned to false.
    /// The above pattern can be very useful when handling user input that would cause navigation. While using <see cref="ICommand.CanExecute(object)"/> is
    /// important to provide visual feedback and to deny things like pressing a button when certain conditions are met, there is a window of time between
    /// when the user does something that causes <c>CanExecute</c> to return <c>false</c> and the time when the UI is actually updated to reflect that. During
    /// that time, a user could press several controls or double tap/click the same control. The result is a race condition or possibly executing the same command
    /// twice (which could result in navigating back two pages when only one was intended, for example). If you guard all of the code that handles commands and 
    /// events that could trigger navigation using the same <see cref="AtomicBoolean"/> using the pattern above, you will prevent the side effects of responding to
    /// more than one of those events at the same time.
    /// Using a plain <c>bool</c> as a guard is insufficient.
    /// <code>
    /// // WARNING: DO NOT DO THIS
    /// if (!guardBool) { guardBool = true; try { runMyCode(); } finally { guardBool = false; } }
    /// </code>
    /// </summary>
    /// Between the time your code checks the value of <c>guardBool</c> and the time it sets it to <c>true</c>, other code could have run that also checked 
    /// <c>guardBool</c> and determined that it was "safe" to run because <c>guardBool</c> was (still) false. The methods in <see cref="Interlocked"/>, when 
    /// properly used, run in a way (usually by using special CPU instructions meant precisely for this) that it is impossible for the race condition above 
    /// to happen. If you look at the API and documentation, you might wonder why <see cref="Interlocked"/> does not simply provide the ability to use a 
    /// <c>bool</c>, removing the need for a class like this. The answer is essentially that the way those special CPU instructions are implemented make it
    /// impossible to do that (they can only operate on 4 byte and 8 byte values and making all bools into 4 bytes just for this special use case would be 
    /// bad in so many ways).
    /// 
    /// Notes: 
    /// 
    /// All methods in this type are non-blocking.
    /// 
    /// All methods in this type are thread-safe.
    /// 
    /// This type is not a lock. If you need a lock, use one of the types that provide one (see: https://docs.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives ).
    /// 
    /// Using this type as a guard only works if you make sure to use the same instance of it as your guard and make sure you use it everywhere you need to guard.
    /// 
    /// THIS DOES NOT ALLOW FOR RE-ENTRANCY. If you use this to try to guard against certain code running twice and somewhere in the code that runs you try 
    /// to use this same instance to block some other code from running more than once, that other code will never run because it will check to see if the value 
    /// of this is false, this will report that its current value is true and so that code will not run. What you most likely want there is a lock. You can use this
    /// as a guard and then use one or more locks within the guarded code. The various lock types (see the link above) provide thread-safety so that the same
    /// resource cannot be accessed from other threads, but they allow the same thread access to that resource. So you cannot rely on locks to prevent some code from 
    /// running while other code is running because the code could be sharing a thread with the system switching between the different code for optimization 
    /// reasons, etc.
    /// 
    /// To sum up, this can be used as a guard but not as a lock. If you use this as a guard, make sure that none of the code you call behind the guard attempts to
    /// use the same instance of this as a guard because it will never run; you should either be using a different instance of this as a guard for that, be using 
    /// a type that provides a lock for that, or need to refactor your code in some other way so you don't try to use this as a guard when it's already in use for 
    /// some other purpose.
    public class AtomicBoolean
    {
        private const int m_falseValue = 0;
        private const int m_trueValue = 1;
        private int m_boolValue;

        public AtomicBoolean(bool initialValue = false)
        {
            m_boolValue = initialValue ? m_trueValue : m_falseValue;
        }

        /// <summary>
        /// Gets the current value of this.
        /// </summary>
        /// <returns>The current value of this.</returns>
        /// <remarks>
        /// This only performs an immediate check on the current value. The value could change before control even returns to the method calling this.
        /// Because of this, the uses for this method are very limited. You should not use it to do something like:
        /// <code>
        /// // WARNING: DO NOT DO THIS
        /// if (!ab.Get()) { ab.Set(true); runMyCode(); ab.Set(false); }
        /// </code>
        /// That is not safe. Instead you should use something like this:
        /// <code>
        /// if (!ab.Set(true)) { try { runMyCode(); } finally { ab.ForceToFalse(); } }
        /// </code>
        /// The use of try-finally (or try-catch-finally) is critical when using the above pattern. If an exception is thrown during the execution of <c>runMyCode</c>, 
        /// without ensuring that the value of <c>ab</c> is set back to <c>false</c> in a finally block could result in the value of <c>ab</c> remaining <c>true</c>. 
        /// Any subsequent use of <c>ab</c> in the same way as above would result in your code never running since <c>ab</c> would always be <c>true</c>. And you would
        /// never be able to detect if <c>ab</c> was true because of a failure to set it to false somewhere else or because it was legitimately being used by some other
        /// code on another thread.
        /// </remarks>
        public bool Get()
        {
            return Interlocked.CompareExchange(ref m_boolValue, m_trueValue, m_trueValue) == m_trueValue;
        }

        /// <summary>
        /// Forces the value of this to be false regardless of its current value. It is functionally equivalent to
        /// <c>Set(false)</c> but skips a branch that <see cref="Set(bool)"/> requires in order to return the previous
        /// value.
        /// </summary>
        public void ForceToFalse()
        {
            _ = Interlocked.Exchange(ref m_boolValue, m_falseValue);
        }

        /// <summary>
        /// Sets the current value of this to <paramref name="value"/> and returns the previous value of this. If the previous value
        /// and <paramref name="value"/> are the same, the value of this remains unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The previous value.</returns>
        public bool Set(bool value)
        {
            return value
                ? Interlocked.CompareExchange(ref m_boolValue, m_trueValue, m_falseValue) == m_trueValue
                : Interlocked.CompareExchange(ref m_boolValue, m_falseValue, m_trueValue) == m_trueValue;
        }
    }
}
