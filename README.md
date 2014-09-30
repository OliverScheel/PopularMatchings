PopularMatchings
================

This repository contains the C# program PopularMatchings which has been written during the course of my bachelor thesis regarding popular matchings.
The folder "Source Code" contains the source files, "Thesis" the thesis in which also the usage of the program is described in the Appendix.
In "Executable" I want to provide an executable, but currently I have problems with that. When providing an installer I get an incorrect Hash error,
therefore I just zipped the compiled Release folder. This seems to work except Gurobi then has errors due to missing DLLs. It might work if you have Gurobi
installed on your machine.
To compile your own executable and run the program, open the source files in Visual Studio and compile them. For this you need to have the optimizer Gurobi for 32 bit installed.

Oliver Scheel
oliver.scheel@rwth-aachen.de


