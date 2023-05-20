using System;
using System.Collections.Generic;
using System.Text;

namespace ChatPalette
{
    class Config
    {
            public struct tabInfos
            {
                string tabName;
                string color;
                List<string> lines;
                string name;
            }

            string saveDataTypeName;
            List<tabInfos> savedata;
    }
}
