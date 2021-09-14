using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace PostBotPrime
{
    public class Speech
    {
       public bool ReadPosts(Form1._Pbox[] posts)
        {
            SpeechSynthesizer Voice1 = new SpeechSynthesizer();
            Voice1.SetOutputToDefaultAudioDevice();
            Voice1.SelectVoiceByHints(VoiceGender.Male);

            for (int i = 0; i < posts.Length; i++)
            {
                Voice1.Speak(posts[i].Data);
            }
            return true;
        }
    }
}
