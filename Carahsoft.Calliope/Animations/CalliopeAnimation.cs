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
        public CalliopePrintOptions Options { get; }

        protected CalliopeAnimation(string renderText, CalliopePrintOptions options)
        {
            RenderText = renderText;
            Options = options;
        }

        public abstract CalliopeCmd? Init();

        public abstract CalliopeCmd? Update(CalliopeMsg msg);

        public abstract string View();


        public static RainbowAnimation RainbowAnimation(string renderText, CalliopePrintOptions options)
            => new RainbowAnimation(renderText, options);

        public static TwinkleAnimation TwinkleAnimation(string renderText, CalliopePrintOptions options)
            => new TwinkleAnimation(renderText, options);

        public static BlizzardAnimation RainAnimation(string renderText, CalliopePrintOptions options)
    => new BlizzardAnimation(renderText, options);
    }
}
