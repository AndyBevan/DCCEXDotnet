# DCCEXDotnet

# Origins and Compatability Design Principals
DCCEXDotnet is a dotnet port of the fantastic DCCExProtocol - https://github.com/DCC-EX/DCCEXProtocol.  DCCEXProtocol is written in C++.  Efforts have been made to keep the .Net code in sync with the original to make any future updates easier when the C++ code changes.  Examples of this are using GetId() methods rather than a public property Id.  

There are places where the .Net code is not as elegant as could be possible until I get time to refactor it.  One particular example is the static instances of _first throughout the main classes.  This is biproduct of the original code where this wasn't a problem.  I will likely come back and fix this later.  For now you have to run the tests not in parallel to accomodate for this (See the DisableTestParallelization flag in the tests project)

I've also added some klunky GetByIdNonStatic to get it running.  This is an instance method accessing a static variable which could lead to some nasty issues.

## TODO Notes - Fixes Needed

- Class Loco
-- private static Loco _first;
--- I'm not clear why this would need to be static - need another approach for a single instance

- Class DccInbound
-- Needs to have the static nature removed

- Class Routes
-- GetById - I've got a static and an instance (GetByIdNonStatic) here - for compatability

Turnouts
-- GetById and GetByIdNonStatic

# License
I've kept the license aligned to the original source code that this was based.

Creative Commons [CC-BY-SA 4.0][CCBYSA]   ![CCBYSA](https://i.creativecommons.org/l/by-sa/4.0/88x31.png)

**Free Software, Oh Yeah!**

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

   [depinj]: <https://en.wikipedia.org/wiki/Dependency_injection>
   [delegate]: <https://en.wikipedia.org/wiki/Delegation_(object-oriented_programming)>
   [CCBYSA]: <http://creativecommons.org/licenses/by-sa/4.0/>