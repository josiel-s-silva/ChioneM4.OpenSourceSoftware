using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal class Utils
{
    public static byte NumberToByte(int num)
    {
        return num switch
        {
            0 => 247,
            1 => 145,
            2 => 235,
            3 => 187,
            4 => 157,
            5 => 190,
            6 => 254,
            7 => 151,
            8 => 255,
            9 => 191,
            10 => 136,
            _ => 136,
        };
    }


    public static byte[] UpDisplay(int v, int display_type)
    {
        var result = new byte[5];
        if (v <= 0)
            return result;

        var blocks = new (int start, byte[] value)[] {};

        if (display_type == 0)
        {
            blocks = new (int start, byte[] value)[]
            {
            (1,  new byte[] {128, 0, 0, 0, 0}),
            (6,  new byte[] {64, 0, 0, 0, 0}),
            (11, new byte[] {32, 0, 0, 0, 0}),
            (16, new byte[] {16, 0, 0, 0, 0}),
            (21, new byte[] {8, 0, 0, 0, 0}),
            (26, new byte[] {4, 0, 0, 0, 0}),
            (31, new byte[] {2, 0, 0, 0, 0}),
            (36, new byte[] {1, 0, 0, 0, 0}),
            (41, new byte[] {0, 128, 0, 0, 0}),
            (46, new byte[] {0, 64, 0, 0, 0}),
            (51, new byte[] {0, 32, 0, 0, 0}),
            (56, new byte[] {0, 16, 0, 0, 0}),
            (61, new byte[] {0, 8, 0, 0, 0}),
            (66, new byte[] {0, 4, 0, 0, 0}),
            (71, new byte[] {0, 2, 0, 0, 0}),
            (76, new byte[] {0, 1, 0, 0, 0}),
            (81, new byte[] {0, 0, 128, 0, 0}),
            (86, new byte[] {0, 0, 64, 0, 0}),
            (91, new byte[] {0, 0, 32, 0, 0}),
            (96, new byte[] {0, 0, 16, 0, 0}),
            };
        }
        else
        {
            blocks = new (int start, byte[] value)[]
            {
            (1,  new byte[] {128, 0, 0, 0, 0}),
            (6,  new byte[] {192, 0, 0, 0, 0}),
            (11, new byte[] {224, 0, 0, 0, 0}),
            (16, new byte[] {240, 0, 0, 0, 0}),
            (21, new byte[] {248, 0, 0, 0, 0}),
            (26, new byte[] {252, 0, 0, 0, 0}),
            (31, new byte[] {254, 0, 0, 0, 0}),
            (36, new byte[] {255, 0, 0, 0, 0}),
            (41, new byte[] {255, 128, 0, 0, 0}),
            (46, new byte[] {255, 192, 0, 0, 0}),
            (51, new byte[] {255, 224, 0, 0, 0}),
            (56, new byte[] {255, 240, 0, 0, 0}),
            (61, new byte[] {255, 248, 0, 0, 0}),
            (66, new byte[] {255, 252, 0, 0, 0}),
            (71, new byte[] {255, 254, 0, 0, 0}),
            (76, new byte[] {255, 255, 0, 0, 0}),
            (81, new byte[] {255, 255, 128, 0, 0}),
            (86, new byte[] {255, 255, 192, 0, 0}),
            (91, new byte[] {255, 255, 224, 0, 0}),
            (96, new byte[] {255, 255, 240, 0, 0}),
            };
        }
        foreach (var block in blocks)
        {
            if (v >= block.start && v <= block.start + 4)
            {
                return block.value;
            }
        }

        return result;
    }
}