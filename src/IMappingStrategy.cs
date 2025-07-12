using System;

namespace Sufficit.Asterisk.FastAGI
{
    public interface IMappingStrategy
    {
        AGIScript? DetermineScript(AGIRequest request);

        void Load();
    }
}