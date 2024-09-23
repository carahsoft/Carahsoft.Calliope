using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    public static class CalliopeAnimation
    {
        public static RainbowAnimation RainbowAnimation(string renderText, CalliopeOptions options)
            => new RainbowAnimation(renderText, options);

        public static TwinkleAnimation TwinkleAnimation(string renderText, CalliopeOptions options)
            => new TwinkleAnimation(renderText, options);
    }

    public abstract class CalliopeAnimation<T> : ICalliopeProgram<T>
    {
        public string RenderText { get; }
        public CalliopeOptions Options { get; }

        protected CalliopeAnimation(string renderText, CalliopeOptions options)
        {
            RenderText = renderText;
            Options = options;
        }

        public abstract (T, CalliopeCmd?) Init();

        public abstract (T, CalliopeCmd?) Update(T state, CalliopeMsg msg);

        public abstract string View(T state);
    }
}
