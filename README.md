# arcomp

arcomp is a simple command-line utility used to compare archives or show their contents. It uses parsed 7-Zip output, and is designed mainly as a showcase for underlying library. Password-protected archives are not supported.

### Usage examples

Shows properties and entries of one or more archives:

    arcomp -s "archive 1.zip" "..\another archive 2.rar" "C:\some folder\other archive 3.7z"

Compares two archives and shows their difference in properties and entries:

    arcomp -c "archive 1.zip" "..\another archive 2.rar"



### Options

    -s, --show      When given one or more archives, will show their properties and entries. Expects one or more archive quoted filenames divided by space: "archive 1.zip" "..\another archive 2.rar" "C:\some folder\other archive 3.7z"

    -c, --compare   When given two archives, will show their differences in properties and entries. Expects exactly two archive quoted filenames divided by space: "archive 1.zip" "..\another archive 2.rar"

    --help          Displays help screen.


### NuGet references
You may notice that NuGet packages are not in the repository, so do not forget to set up package restoration in Visual Studio:

Tools menu → Options → Package Manager → General → "Allow NuGet to download missing packages during build" should be selected.

If you have a build server then it needs to be setup with an environment variable 'EnableNuGetPackageRestore' set to true.