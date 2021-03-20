using System;
using System.Collections.Generic;
using System.Text;

namespace Chatparet
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
