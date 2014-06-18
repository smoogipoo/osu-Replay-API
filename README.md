osu! Replay API
===============

An API for quick read and write to osu! Replay Files.


What you need to be able to use it:
-----------------------------------
.NET Framework 3.5


Directions for use:
-------------------
To use this API you must add a reference to the DLL file in your project (Project -> Add Reference) and add 'using ReplayAPI' (or the equivalent for your language) to the top of your project.

If you are working with the source files instead (thus working in C#) and compiling the API along with your program, you do not need to add a reference to the DLL file, however the API files must be added to your project (Right click solution -> Add -> Existing Item).


Usage example:
--------------
```C#
			//Load the replay into a new replay object
            ReplayAPI.Replay rep = new ReplayAPI.Replay(@"C:\MyReplayFile.osr");

            //We do not need to load the entire replay, just the score-screen contents
            rep.LoadMetadata();

            //Lets do some pretty printing!
            Console.WriteLine("You've hit {0} 300s, {1} 100s, {2} 50s and {3} misses.", rep.Count_300, rep.Count_100, rep.Count_50, rep.Count_Miss);
            Console.WriteLine("Out of your 300 hits and 100 hits, you hit {0} Gekis and {1} Katus.", rep.Count_Geki, rep.Count_Katu);
```	
	
License
-------
The MIT License (MIT)

Copyright (c) 2014 smoogipooo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.