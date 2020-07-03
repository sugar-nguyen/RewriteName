using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RewriteName
{
    public static class PlayMucsic
    {
       private static MediaPlayer media;
        public static int Play()
        {
            string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources", "music.wav");
            if (File.Exists(path))
            {
                media = new MediaPlayer();
                media.Open(new Uri(path));
                media.Play();
                return 1;
            }
            return 0;
        }
        public static void Play(string path)
        {
            media = new MediaPlayer();
            media.Open(new Uri(path));
            media.Play();
        }
    }
}
