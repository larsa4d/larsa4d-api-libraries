Using Python to write LARSA 4D Macros
=====================================

The [Python](https://www.python.org/) programming language can be used to write macros for LARSA 4D.

Since Python does not have a code editor like the VBA code editor in Microsoft Excel which provides hints about object class, property, and method names, it is hard to know ahead of time which methods and properties you can use. It is recommended to prototype a macro in VBA in Excel first, or contact the LARSA support team for help.

Preparing Your Workstation
--------------------------

* Install the latest Windows installer for Python from https://www.python.org/downloads/windows/. Because LARSA 4D's COM interface is 32-bit, there are some methods of the COM interface that cannot be used with a 64-bit version of Python. For that reason, the 32-bit installer is recommended.

In a Command Prompt (Windows start menu -> Command Prompt), run these commands:

```
py -m pip install pywin32

py -m win32com.client.makepy "LARSA 2000 Library Elements"
```

The first command installs the open source pywin32 package which enables connecting to COM-enabled applications in Python. The second command runs a tool it provides for it to learn about the LARSA 4D COM interface. (Note that if you have multiple versions of Python installed, you may need to select a version. Try running `py --help` and `py --list` for additional instructions.)

You can then start Python's IDLE tool from the Windows start menu.

After running the third command above, it will output that it's creating a file and will give the path to that file. Looking inside that file can help find the names of constants and methods for the LARSA 4D interface, although it’s quite complex.

Macro Approaches
----------------

There are two approaches for writing LARSA 4D macros. Examples will be given for each. (The examples, and the two approaches for using the LARSA 4D COM interface, correspond exactly to how macros are written in VBA in Excel.)

### 1) Using the LarsaElements library directly

The first approach uses the "LarsaElements library" directly. This approach has the following benefits:

* It is faster than the second approach when creating or accessing a very large amount of project or results information.
* LARSA 4D .lar files can be read and written without LARSA 4D being open.

It has the following limitations:

* A 32-bit version of Python is required.
* Live-updating a project that is open in LARSA 4D is not possible.

Example: [LARSA4D_Macro_Approach1.py](LARSA4D_Macro_Approach1.py)

### 2) Using the Application object

The second approach manipulates a running instance of LARSA 4D using the LARSA 4D Application object. It has the following benefits:

* It is possible to live-update a project open in a running instance of LARSA 4D.
* A 64-bit version of Python can be used.

It has the following limitations:

* It is slower than the first approach when creating or accessing a very large amount of project or results information.
* Only the project open in the running instance of LARSA 4D can be modified.

Example: [LARSA4D_Macro_Approach2.py](LARSA4D_Macro_Approach2.py) 
