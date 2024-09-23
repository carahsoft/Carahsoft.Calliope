using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carahsoft.Calliope.Animations
{
    public abstract class CalliopeAnimation : ICalliopeProgram
    {
        public string RenderText { get; }
        public CalliopeOptions Options { get; }

        protected CalliopeAnimation(string renderText, CalliopeOptions options)
        {
            RenderText = renderText;
            Options = options;
        }

        public abstract CalliopeCmd? Init();

        public abstract CalliopeCmd? Update(CalliopeMsg msg);

        public abstract string View();


        public static RainbowAnimation RainbowAnimation(string renderText, CalliopeOptions options)
            => new RainbowAnimation(renderText, options);

        public static TwinkleAnimation TwinkleAnimation(string renderText, CalliopeOptions options)
            => new TwinkleAnimation(renderText, options);

        public static BlizzardAnimation RainAnimation(string renderText, CalliopeOptions options)
    => new BlizzardAnimation(renderText, options);
    }
}
